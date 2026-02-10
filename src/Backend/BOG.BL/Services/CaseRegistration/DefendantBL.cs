using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DTO.CaseRegistration.Defendant;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Lookups;
using BOG.VM.Defendant;
using CaseRequestDefendant = BOG.DbModel.Entities.CaseRegistration.CaseRequestDefendant;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for defendant management in case registration.
/// Implements defendant creation, retrieval, updates, and deletion.
/// Enforces business rule ERR013 (duplicate identity check).
/// </summary>
public class DefendantBL : IDefendantBL
{
    private readonly IDefendantRepository _defendantRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DefendantBL(
        IDefendantRepository defendantRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _defendantRepository = defendantRepository ?? throw new ArgumentNullException(nameof(defendantRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Creates a new defendant for a case registration request.
    /// Validates business rules including ERR013 (duplicate identity check).
    /// </summary>
    public async Task<DefendantVM> CreateDefendantAsync(int requestId, object defendantData, CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (defendantData == null)
            throw new ArgumentNullException(nameof(defendantData));

        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        // Handle both strongly-typed DTO and object types
        DefendantCreateDTO createDto;
        if (defendantData is DefendantCreateDTO typedDto)
        {
            createDto = typedDto;
        }
        else if (defendantData is IDictionary<string, object> dictData)
        {
            createDto = new DefendantCreateDTO
            {
                DefendantTypeId = Convert.ToInt32(dictData["defendantTypeId"]),
                FullName = dictData.ContainsKey("fullName") ? dictData["fullName"]?.ToString() ?? "" : "",
                IdentityNumber = dictData.ContainsKey("identityNumber") ? dictData["identityNumber"]?.ToString() : null,
                IdentityTypeId = dictData.ContainsKey("identityTypeId") ? (int?)Convert.ToInt32(dictData["identityTypeId"]) : null,
                AddressText = dictData.ContainsKey("addressText") ? dictData["addressText"]?.ToString() : null,
                CommercialRegNumber = dictData.ContainsKey("commercialRegNumber") ? dictData["commercialRegNumber"]?.ToString() : null,
                GovernmentAgencyId = dictData.ContainsKey("governmentAgencyId") ? (int?)Convert.ToInt32(dictData["governmentAgencyId"]) : null,
                AdditionalStatement = dictData.ContainsKey("additionalStatement") ? dictData["additionalStatement"]?.ToString() : null
            };
        }
        else
        {
            throw new ArgumentException("Invalid defendant data format.", nameof(defendantData));
        }

        // Get request and validate it exists and is not deleted
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null || request.IsDeleted)
            throw new InvalidOperationException($"Request {requestId} not found or has been deleted.");

        // Validate request state allows adding defendants (Draft=1 or PendingCompletion=8)
        if (request.RequestStatusId != 1 && request.RequestStatusId != 8)
            throw new InvalidOperationException("Cannot add defendant - request not in Draft or PendingCompletion state.");

        // Validate required field: FullName
        if (string.IsNullOrWhiteSpace(createDto.FullName))
            throw new ArgumentException("Full name is required.", nameof(createDto.FullName));

        // ERR013: Check duplicate identity (identity number + defendant type must be unique per request)
        if (!string.IsNullOrWhiteSpace(createDto.IdentityNumber))
        {
            var exists = await _defendantRepository.ExistsByIdentityAsync(
                requestId, createDto.IdentityNumber, createDto.DefendantTypeId, cancellationToken);
            if (exists)
                throw new InvalidOperationException("ERR013: Defendant with this identity already exists for this request.");
        }

        // Create defendant entity
        var defendant = new Defendant
        {
            DefendantTypeId = createDto.DefendantTypeId,
            FullName = createDto.FullName,
            IdentityTypeId = createDto.IdentityTypeId,
            IdentityNumber = createDto.IdentityNumber,
            AddressText = createDto.AddressText,
            CommercialRegNumber = createDto.CommercialRegNumber,
            GovernmentAgencyId = createDto.GovernmentAgencyId,
            AdditionalStatement = createDto.AdditionalStatement,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        // Create junction table entry to link defendant to request
        var caseRequestDefendant = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = requestId,
            Defendant = defendant,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        // Add the junction entity (which will cascade to add the defendant)
        defendant.CaseRequestDefendants.Add(caseRequestDefendant);

        // Add defendant to repository
        await _defendantRepository.AddAsync(defendant, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload defendant with navigation properties for mapping
        var savedDefendant = await _defendantRepository.GetByIdAsync(defendant.Id, cancellationToken);
        if (savedDefendant == null)
            throw new InvalidOperationException($"Failed to retrieve created defendant with ID {defendant.Id}");

        // Return view model
        return MapToViewModel(savedDefendant);
    }

    /// <summary>
    /// Gets a defendant by ID.
    /// </summary>
    public async Task<DefendantVM?> GetDefendantByIdAsync(int defendantId, CancellationToken cancellationToken = default)
    {
        if (defendantId <= 0)
            throw new ArgumentException("Invalid defendant ID.", nameof(defendantId));

        var defendant = await _defendantRepository.GetByIdAsync(defendantId, cancellationToken);

        if (defendant == null || defendant.IsDeleted)
            return null;

        return MapToViewModel(defendant);
    }

    /// <summary>
    /// Gets all defendants for a specific case registration request.
    /// </summary>
    public async Task<IEnumerable<DefendantListVM>> GetDefendantsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var defendants = await _defendantRepository.GetByRequestIdAsync(requestId, cancellationToken);

        return defendants.Select(MapToListViewModel).ToList();
    }

    /// <summary>
    /// Updates a defendant.
    /// </summary>
    public async Task<DefendantVM> UpdateDefendantAsync(int defendantId, object defendantData, CancellationToken cancellationToken = default)
    {
        if (defendantId <= 0)
            throw new ArgumentException("Invalid defendant ID.", nameof(defendantId));

        if (defendantData == null)
            throw new ArgumentNullException(nameof(defendantData));

        // Handle both strongly-typed DTO and object types
        DefendantUpdateDTO updateDto;
        if (defendantData is DefendantUpdateDTO typedDto)
        {
            updateDto = typedDto;
        }
        else if (defendantData is IDictionary<string, object> dictData)
        {
            updateDto = new DefendantUpdateDTO
            {
                FullName = dictData.ContainsKey("fullName") ? dictData["fullName"]?.ToString() : null,
                AddressText = dictData.ContainsKey("addressText") ? dictData["addressText"]?.ToString() : null,
                AdditionalStatement = dictData.ContainsKey("additionalStatement") ? dictData["additionalStatement"]?.ToString() : null
            };
        }
        else
        {
            throw new ArgumentException("Invalid defendant data format.", nameof(defendantData));
        }

        // Get existing defendant
        var defendant = await _defendantRepository.GetByIdAsync(defendantId, cancellationToken);
        if (defendant == null || defendant.IsDeleted)
            throw new InvalidOperationException($"Defendant {defendantId} not found.");

        // Update optional fields
        if (!string.IsNullOrWhiteSpace(updateDto.FullName))
            defendant.FullName = updateDto.FullName;

        if (updateDto.AddressText != null)
            defendant.AddressText = updateDto.AddressText;

        if (updateDto.AdditionalStatement != null)
            defendant.AdditionalStatement = updateDto.AdditionalStatement;

        defendant.ModifiedDate = DateTime.UtcNow;

        // Save changes
        await _defendantRepository.UpdateAsync(defendant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToViewModel(defendant);
    }

    /// <summary>
    /// Soft-deletes a defendant.
    /// </summary>
    public async Task DeleteDefendantAsync(int defendantId, CancellationToken cancellationToken = default)
    {
        if (defendantId <= 0)
            throw new ArgumentException("Invalid defendant ID.", nameof(defendantId));

        var defendant = await _defendantRepository.GetByIdAsync(defendantId, cancellationToken);
        if (defendant == null || defendant.IsDeleted)
            throw new InvalidOperationException($"Defendant {defendantId} not found.");

        // Soft delete
        defendant.IsDeleted = true;
        defendant.ModifiedDate = DateTime.UtcNow;

        await _defendantRepository.UpdateAsync(defendant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Maps Defendant entity to view model.
    /// </summary>
    private static DefendantVM MapToViewModel(Defendant defendant)
    {
        if (defendant == null)
            throw new ArgumentNullException(nameof(defendant));

        return new DefendantVM
        {
            Id = defendant.Id,
            DefendantTypeId = defendant.DefendantTypeId,
            DefendantTypeName = defendant.DefendantType?.NameAr ?? "",
            FullName = defendant.FullName,
            IdentityTypeId = defendant.IdentityTypeId,
            IdentityTypeName = defendant.IdentityType?.NameAr,
            IdentityNumber = defendant.IdentityNumber,
            AddressText = defendant.AddressText,
            CommercialRegNumber = defendant.CommercialRegNumber,
            GovernmentAgencyId = defendant.GovernmentAgencyId,
            AdditionalStatement = defendant.AdditionalStatement,
            IsActive = defendant.IsActive,
            CreatedDate = defendant.CreatedDate,
            ModifiedDate = defendant.ModifiedDate
        };
    }

    /// <summary>
    /// Maps Defendant entity to list view model (compact view).
    /// </summary>
    private static DefendantListVM MapToListViewModel(Defendant defendant)
    {
        if (defendant == null)
            throw new ArgumentNullException(nameof(defendant));

        return new DefendantListVM
        {
            Id = defendant.Id,
            FullName = defendant.FullName,
            DefendantTypeName = defendant.DefendantType?.NameAr ?? "",
            IdentityNumber = defendant.IdentityNumber,
            IdentityTypeName = defendant.IdentityType?.NameAr,
            IsActive = defendant.IsActive,
            CreatedDate = defendant.CreatedDate
        };
    }
}
