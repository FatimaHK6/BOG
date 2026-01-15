using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.Representative;
using BOG.Integration.Interfaces;
using BOG.VM.Representative;

namespace BOG.BL.Services;

/// <summary>
/// Representative business logic service.
/// Handles representative CRUD operations and business rules.
/// </summary>
public class RepresentativeBL : IRepresentativeBL
{
    private readonly IRepresentativeRepository _representativeRepository;
    private readonly IPlaintiffRepository _plaintiffRepository;
    private readonly IAbsherService _absherService;
    private readonly IUnitOfWork _unitOfWork;

    // Representative type to plaintiff type mapping
    private static readonly Dictionary<int, int[]> AllowedRepresentativeTypes = new()
    {
        // Individual (1): Lawyer, Liquidator, Trustee, Custodian, Guardian, Conservator
        [1] = new[] { 1, 2, 3, 4, 6, 8 },
        // IndividualNoId (2): Same as Individual
        [2] = new[] { 1, 2, 3, 4, 6, 8 },
        // BusinessOwner (3): Lawyer, Liquidator, Trustee, Custodian
        [3] = new[] { 1, 2, 3, 4 },
        // RegisteredCompany (4): Lawyer, Liquidator, Trustee, Custodian, CompanyRep
        [4] = new[] { 1, 2, 3, 4, 5 },
        // UnregisteredCompany (5): Lawyer, Liquidator, Trustee, Custodian
        [5] = new[] { 1, 2, 3, 4 },
        // GovernmentAgency (6): Lawyer, GovRep
        [6] = new[] { 1, 7 },
        // NGO (7): Lawyer, CompanyRep
        [7] = new[] { 1, 5 },
        // Waqf (8): Lawyer, WaqfInspector
        [8] = new[] { 1, 9 }
    };

    public RepresentativeBL(
        IRepresentativeRepository representativeRepository,
        IPlaintiffRepository plaintiffRepository,
        IAbsherService absherService,
        IUnitOfWork unitOfWork)
    {
        _representativeRepository = representativeRepository ?? throw new ArgumentNullException(nameof(representativeRepository));
        _plaintiffRepository = plaintiffRepository ?? throw new ArgumentNullException(nameof(plaintiffRepository));
        _absherService = absherService ?? throw new ArgumentNullException(nameof(absherService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<RepresentativeVM>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        var representatives = await _representativeRepository.GetByPlaintiffIdAsync(plaintiffId, cancellationToken);
        return representatives.Select(MapToVM).ToList();
    }

    public async Task<RepresentativeVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var representative = await _representativeRepository.GetWithDetailsAsync(id, cancellationToken);
        return representative == null ? null : MapToVM(representative);
    }

    public async Task<RepresentativeVM> CreateAsync(int plaintiffId, RepresentativeCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Get plaintiff to validate
        var plaintiff = await _plaintiffRepository.GetByIdAsync(plaintiffId, cancellationToken) as Plaintiff;
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {plaintiffId} غير موجود");

        // Validate representative type allowed for plaintiff type
        if (!IsRepresentativeTypeAllowed(plaintiff.PlaintiffTypeId, dto.RepresentativeTypeId))
            throw new InvalidOperationException("نوع الممثل غير مسموح لهذا النوع من المدعين");

        // ERR012: Representative identity cannot be same as plaintiff identity
        if (!string.IsNullOrEmpty(dto.IdentityNumber) && dto.IdentityNumber == plaintiff.IdentityNumber)
            throw new InvalidOperationException("ERR012: رقم هوية الممثّل نفس رقم هوية المدّعي");

        // ERR008: Check for duplicate representative
        var exists = await _representativeRepository.ExistsByIdentityAsync(plaintiffId, dto.IdentityNumber, cancellationToken);
        if (exists)
            throw new InvalidOperationException("ERR008: الممثّل موجود مسبقاً للمدعي");

        // Verify with Absher and get data
        int dataSourceId = 2; // Default: FromUser
        var verificationResult = await _absherService.VerifyIdentityAsync(dto.IdentityNumber, dto.IdentityTypeId, cancellationToken);

        if (verificationResult.IsVerified)
        {
            var personData = await _absherService.GetPersonDataAsync(dto.IdentityNumber, dto.IdentityTypeId, cancellationToken);
            if (personData != null)
            {
                dto.FirstName = personData.FirstName;
                dto.FatherName = personData.FatherName;
                dto.GrandfatherName = personData.GrandfatherName;
                dto.FamilyName = personData.FamilyName;
                dto.BirthDate = personData.BirthDate;
                dto.MobileNumber ??= personData.MobileNumber;
                dto.Email ??= personData.Email;
                dataSourceId = 1; // FromAbsher
            }
        }

        // Create representative entity
        var representative = new Representative
        {
            PlaintiffId = plaintiffId,
            RepresentativeTypeId = dto.RepresentativeTypeId,
            IdentityTypeId = dto.IdentityTypeId,
            IdentityNumber = dto.IdentityNumber,
            FirstName = dto.FirstName,
            FatherName = dto.FatherName,
            GrandfatherName = dto.GrandfatherName,
            FamilyName = dto.FamilyName,
            BirthDate = dto.BirthDate,
            DataSourceId = dataSourceId,
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            AuthorizationNumber = dto.AuthorizationNumber,
            AuthorizationDate = dto.AuthorizationDate,
            AuthorizationSource = dto.AuthorizationSource,
            AuthorizationSourceType = dto.AuthorizationSourceType,
            GuardianshipType = dto.GuardianshipType,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _representativeRepository.AddAsync(representative, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(representative.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create representative");
    }

    public async Task<RepresentativeVM> UpdateAsync(int id, RepresentativeUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var representative = await _representativeRepository.GetByIdAsync(id, cancellationToken) as Representative;
        if (representative == null || representative.IsDeleted)
            throw new InvalidOperationException($"الممثل رقم {id} غير موجود");

        // Update fields (only non-Absher fields can be updated if data is from Absher)
        if (representative.DataSourceId != 1) // Not from Absher
        {
            if (!string.IsNullOrEmpty(dto.FirstName)) representative.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.FatherName)) representative.FatherName = dto.FatherName;
            if (!string.IsNullOrEmpty(dto.GrandfatherName)) representative.GrandfatherName = dto.GrandfatherName;
            if (!string.IsNullOrEmpty(dto.FamilyName)) representative.FamilyName = dto.FamilyName;
        }

        // Contact info can always be updated
        if (dto.MobileNumber != null) representative.MobileNumber = dto.MobileNumber;
        if (dto.Email != null) representative.Email = dto.Email;

        // Authorization info
        if (dto.AuthorizationNumber != null) representative.AuthorizationNumber = dto.AuthorizationNumber;
        if (dto.AuthorizationDate.HasValue) representative.AuthorizationDate = dto.AuthorizationDate;
        if (dto.AuthorizationSource != null) representative.AuthorizationSource = dto.AuthorizationSource;
        if (dto.AuthorizationSourceType != null) representative.AuthorizationSourceType = dto.AuthorizationSourceType;

        representative.ModifiedDate = DateTime.UtcNow;

        await _representativeRepository.UpdateAsync(representative, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to update representative");
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var representative = await _representativeRepository.GetByIdAsync(id, cancellationToken) as Representative;
        if (representative == null || representative.IsDeleted)
            throw new InvalidOperationException($"الممثل رقم {id} غير موجود");

        // Soft delete
        representative.IsDeleted = true;
        representative.IsActive = false;
        representative.ModifiedDate = DateTime.UtcNow;

        await _representativeRepository.UpdateAsync(representative, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public IEnumerable<int> GetAllowedRepresentativeTypes(int plaintiffTypeId)
    {
        return AllowedRepresentativeTypes.TryGetValue(plaintiffTypeId, out var types)
            ? types
            : Array.Empty<int>();
    }

    private bool IsRepresentativeTypeAllowed(int plaintiffTypeId, int representativeTypeId)
    {
        return AllowedRepresentativeTypes.TryGetValue(plaintiffTypeId, out var allowedTypes)
            && allowedTypes.Contains(representativeTypeId);
    }

    private static RepresentativeVM MapToVM(Representative rep)
    {
        return new RepresentativeVM
        {
            Id = rep.Id,
            PlaintiffId = rep.PlaintiffId,
            RepresentativeTypeId = rep.RepresentativeTypeId,
            RepresentativeTypeNameAr = rep.RepresentativeType?.NameAr ?? "",
            RepresentativeTypeName = rep.RepresentativeType?.Name ?? "",
            IdentityTypeName = rep.IdentityType?.NameAr ?? "",
            IdentityNumber = rep.IdentityNumber,
            FirstName = rep.FirstName,
            FatherName = rep.FatherName,
            GrandfatherName = rep.GrandfatherName,
            FamilyName = rep.FamilyName,
            BirthDate = rep.BirthDate,
            DataSourceId = rep.DataSourceId,
            DataSourceName = rep.DataSource?.NameAr,
            MobileNumber = rep.MobileNumber,
            Email = rep.Email,
            AuthorizationNumber = rep.AuthorizationNumber,
            AuthorizationDate = rep.AuthorizationDate,
            AuthorizationSource = rep.AuthorizationSource,
            AuthorizationSourceType = rep.AuthorizationSourceType,
            GuardianshipType = rep.GuardianshipType,
            CreatedDate = rep.CreatedDate
        };
    }
}
