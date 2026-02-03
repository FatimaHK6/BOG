using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Common;
using BOG.DTO.Common;
using BOG.DTO.Plaintiff;
using BOG.Integration.Interfaces;
using BOG.VM.Common;
using BOG.VM.Plaintiff;
using BOG.VM.Representative;
using Microsoft.Extensions.Logging;

namespace BOG.BL.Services;

/// <summary>
/// Plaintiff business logic service.
/// Handles plaintiff CRUD operations and business rules.
/// </summary>
public class PlaintiffBL : IPlaintiffBL
{
    private readonly IPlaintiffRepository _plaintiffRepository;
    private readonly ICaseRequestPlaintiffRepository _caseRequestPlaintiffRepository;
    private readonly IRepresentativeRepository _representativeRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IAbsherService _absherService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlaintiffBL> _logger;

    // Plaintiff type constants
    private const int PlaintiffTypeIndividual = 1;
    private const int PlaintiffTypeIndividualNoId = 2;

    public PlaintiffBL(
        IPlaintiffRepository plaintiffRepository,
        ICaseRequestPlaintiffRepository caseRequestPlaintiffRepository,
        IRepresentativeRepository representativeRepository,
        IAddressRepository addressRepository,
        IAbsherService absherService,
        IUnitOfWork unitOfWork,
        ILogger<PlaintiffBL> logger)
    {
        _plaintiffRepository = plaintiffRepository ?? throw new ArgumentNullException(nameof(plaintiffRepository));
        _caseRequestPlaintiffRepository = caseRequestPlaintiffRepository ?? throw new ArgumentNullException(nameof(caseRequestPlaintiffRepository));
        _representativeRepository = representativeRepository ?? throw new ArgumentNullException(nameof(representativeRepository));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _absherService = absherService ?? throw new ArgumentNullException(nameof(absherService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<PlaintiffListVM>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var plaintiffs = await _plaintiffRepository.GetByRequestIdAsync(requestId, cancellationToken);
        return plaintiffs.Select(MapToListVM).ToList();
    }

    public async Task<PlaintiffVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var plaintiff = await _plaintiffRepository.GetWithDetailsAsync(id, cancellationToken);
        return plaintiff == null ? null : MapToVM(plaintiff);
    }

    public async Task<PlaintiffVM> CreateAsync(int requestId, PlaintiffCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // ERR011: Check for duplicate plaintiff by identity
        if (!string.IsNullOrEmpty(dto.IdentityNumber))
        {
            var exists = await _plaintiffRepository.ExistsByIdentityAsync(
                requestId, dto.IdentityNumber, dto.PlaintiffTypeId, cancellationToken);
            if (exists)
                throw new InvalidOperationException("ERR011: المدّعي موجود مسبقاً بنفس رقم الهوية");
        }

        // ERR011: Check for duplicate plaintiff by document number (Type 2 - Individual without ID)
        if (dto.PlaintiffTypeId == PlaintiffTypeIndividualNoId && !string.IsNullOrEmpty(dto.DocumentNumber))
        {
            var existsByDoc = await _plaintiffRepository.ExistsByDocumentNumberAsync(
                requestId, dto.DocumentNumber, cancellationToken);
            if (existsByDoc)
                throw new InvalidOperationException("ERR011: المدّعي موجود مسبقاً بنفس رقم الوثيقة");
        }

        // For Individual type, integrate with Absher
        int dataSourceId = 2; // Default: FromUser
        if (dto.PlaintiffTypeId == PlaintiffTypeIndividual && !string.IsNullOrEmpty(dto.IdentityNumber))
        {
            var verificationResult = await _absherService.VerifyIdentityAsync(
                dto.IdentityNumber, dto.IdentityTypeId ?? 1, cancellationToken);

            if (verificationResult.IsVerified)
            {
                // Fetch personal data from Absher
                var personData = await _absherService.GetPersonDataAsync(
                    dto.IdentityNumber, dto.IdentityTypeId ?? 1, cancellationToken);

                if (personData != null)
                {
                    dto.FirstName = personData.FirstName;
                    dto.FatherName = personData.FatherName;
                    dto.GrandfatherName = personData.GrandfatherName;
                    dto.FamilyName = personData.FamilyName;
                    dto.BirthDate = personData.BirthDate;
                    dto.Gender = personData.Gender;
                    dto.MobileNumber ??= personData.MobileNumber;
                    dto.Email ??= personData.Email;
                    dataSourceId = 1; // FromAbsher
                }
            }
        }

        // Create addresses from DTO FIRST (before plaintiff) so FK can be set immediately
        int? residenceAddressId = null;
        int? workAddressId = null;
        int? businessAddressId = null;
        int? companyAddressId = null;
        int? ngoAddressId = null;
        int? waqfAddressId = null;
        int? selectedAddressId = null;

        // Residence Address
        if (dto.ResidenceAddress != null && dto.ResidenceAddress.RegionId > 0 && dto.ResidenceAddress.CityId > 0)
        {
            var address = CreateAddressFromDTO(dto.ResidenceAddress, "Residence");
            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            residenceAddressId = address.Id;
        }

        // Work Address
        if (dto.WorkAddress != null && dto.WorkAddress.RegionId > 0 && dto.WorkAddress.CityId > 0)
        {
            var address = CreateAddressFromDTO(dto.WorkAddress, "Work");
            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            workAddressId = address.Id;
        }

        // Business Address (for Business Owner - Type 3)
        if (dto.BusinessAddress != null && dto.BusinessAddress.RegionId > 0 && dto.BusinessAddress.CityId > 0)
        {
            var address = CreateAddressFromDTO(dto.BusinessAddress, "Business");
            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            businessAddressId = address.Id;
        }

        // Company Address (for Registered Company - Type 4)
        if (dto.CompanyAddress != null && dto.CompanyAddress.RegionId > 0 && dto.CompanyAddress.CityId > 0)
        {
            var address = CreateAddressFromDTO(dto.CompanyAddress, "Company");
            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            companyAddressId = address.Id;
        }

        // NGO Address (for NGO - Type 7)
        if (dto.NGOAddress != null && dto.NGOAddress.RegionId > 0 && dto.NGOAddress.CityId > 0)
        {
            var address = CreateAddressFromDTO(dto.NGOAddress, "NGO");
            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            ngoAddressId = address.Id;
        }

        // Waqf Address (for Waqf - Type 8)
        if (dto.WaqfAddress != null && dto.WaqfAddress.RegionId > 0 && dto.WaqfAddress.CityId > 0)
        {
            var address = CreateAddressFromDTO(dto.WaqfAddress, "Waqf");
            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            waqfAddressId = address.Id;
        }

        // Selected Address (العنوان المختار - for correspondence)
        if (dto.SelectedAddress != null && dto.SelectedAddress.RegionId > 0 && dto.SelectedAddress.CityId > 0)
        {
            var address = CreateAddressFromDTO(dto.SelectedAddress, "Selected");
            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            selectedAddressId = address.Id;
        }

        // === DEBUG: Log before creating entity ===
        _logger.LogWarning("=== CREATING PLAINTIFF ENTITY ===");
        _logger.LogWarning("PlaintiffTypeId: {TypeId}", dto.PlaintiffTypeId);

        // Type 5 (Unregistered Company) fields - DEBUG
        _logger.LogWarning("=== BL: TYPE 5 FIELDS FROM DTO ===");
        _logger.LogWarning("BL DTO CountryId: {CountryId}", dto.CountryId);
        _logger.LogWarning("BL DTO UnregisteredCompanyCity: {City}", dto.UnregisteredCompanyCity);
        _logger.LogWarning("BL DTO Description: {Desc}", dto.Description);
        _logger.LogWarning("BL DTO CompanyName: {Name}", dto.CompanyName);
        _logger.LogWarning("BL DTO CommercialRegNumber: {RegNum}", dto.CommercialRegNumber);

        _logger.LogWarning("Waqf Fields - WaqfName: {WaqfName}, CourtDeedNumber: {Deed}, DeedDate: {Date}",
            dto.WaqfName, dto.CourtDeedNumber, dto.DeedDate);
        _logger.LogWarning("Waqf Fields - DeedSource: {Source}, OversightType: {Type}, Description: {Desc}",
            dto.DeedSource, dto.WaqfOversightType, dto.WaqfDescription);
        _logger.LogWarning("WaqfAddressId will be: {AddressId}", waqfAddressId);

        // Create plaintiff entity with address FKs already set
        var plaintiff = new Plaintiff
        {
            PlaintiffTypeId = dto.PlaintiffTypeId,
            IdentityTypeId = dto.IdentityTypeId,
            IdentityNumber = dto.IdentityNumber,
            FirstName = dto.FirstName,
            FatherName = dto.FatherName,
            GrandfatherName = dto.GrandfatherName,
            ClanName = dto.ClanName,
            FamilyName = dto.FamilyName,
            BirthDate = dto.BirthDate,
            Gender = dto.Gender,
            NationalityId = dto.NationalityId,
            IdentityIssueDate = dto.IdentityIssueDate,
            IdentityExpiryDate = dto.IdentityExpiryDate,
            DataSourceId = dataSourceId,
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            EmploymentStatusId = dto.EmploymentStatusId,
            Employer = dto.Employer,
            Profession = dto.Profession,
            // Business/Company fields
            CommercialRegNumber = dto.CommercialRegNumber,
            CompanyName = dto.CompanyName,
            CRStartDate = dto.CRStartDate,
            CREndDate = dto.CREndDate,
            // Unregistered Company fields
            UnregisteredCompanyAddress = dto.UnregisteredCompanyAddress,
            CountryId = dto.CountryId,
            UnregisteredCompanyCity = dto.UnregisteredCompanyCity,
            Description = dto.Description,
            // Government Agency fields
            GovernmentAgencyId = dto.GovernmentAgencyId,
            Headquarters = dto.Headquarters,
            AdditionalStatement = dto.AdditionalStatement,
            // NGO fields
            LicenseNumber = dto.LicenseNumber,
            LicenseSourceId = dto.LicenseSourceId,
            NGOName = dto.NGOName,
            LicenseDate = dto.LicenseDate,
            // Waqf fields
            CourtDeedNumber = dto.CourtDeedNumber,
            WaqfName = dto.WaqfName,
            DeedDate = dto.DeedDate,
            DeedSource = dto.DeedSource,
            WaqfOversightType = dto.WaqfOversightType,
            WaqfAgencyName = dto.WaqfAgencyName,
            WaqfDescription = dto.WaqfDescription,
            // Individual without ID (Type 2)
            DocumentNumber = dto.DocumentNumber,
            // Address FKs - set from addresses created above
            ResidenceAddressId = residenceAddressId,
            WorkAddressId = workAddressId,
            BusinessAddressId = businessAddressId,
            CompanyAddressId = companyAddressId,
            NGOAddressId = ngoAddressId,
            WaqfAddressId = waqfAddressId,
            SelectedAddressId = selectedAddressId,
            // Additional Data (بيانات إضافية)
            IsApplicant = dto.IsApplicant,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _plaintiffRepository.AddAsync(plaintiff, cancellationToken);
        var rowsSaved = await _unitOfWork.SaveChangesAsync(cancellationToken);

        // === DEBUG: Log after save ===
        _logger.LogWarning("=== PLAINTIFF SAVED TO DB ===");
        _logger.LogWarning("Rows saved: {Rows}, Plaintiff ID: {Id}", rowsSaved, plaintiff.Id);

        // Type 5 (Unregistered Company) fields after save
        _logger.LogWarning("=== BL: TYPE 5 FIELDS AFTER SAVE ===");
        _logger.LogWarning("Saved CountryId: {CountryId}", plaintiff.CountryId);
        _logger.LogWarning("Saved UnregisteredCompanyCity: {City}", plaintiff.UnregisteredCompanyCity);
        _logger.LogWarning("Saved Description: {Desc}", plaintiff.Description);
        _logger.LogWarning("Saved CompanyName: {Name}", plaintiff.CompanyName);

        _logger.LogWarning("Saved Waqf Fields - WaqfName: {WaqfName}, CourtDeedNumber: {Deed}",
            plaintiff.WaqfName, plaintiff.CourtDeedNumber);
        _logger.LogWarning("Saved Waqf Fields - WaqfAddressId: {AddressId}", plaintiff.WaqfAddressId);

        // Create junction table entry
        var caseRequestPlaintiff = new CaseRequestPlaintiff
        {
            CaseRegistrationRequestId = requestId,
            PlaintiffId = plaintiff.Id,
            CreatedDate = DateTime.UtcNow
        };

        await _caseRequestPlaintiffRepository.AddAsync(caseRequestPlaintiff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Clear other applicants if this one is set as applicant
        if (dto.IsApplicant)
        {
            await ClearOtherApplicantsAsync(plaintiff.Id, cancellationToken);
        }

        // Fetch addresses from Absher if available (only if no addresses from form)
        bool hasAddressFromForm = residenceAddressId.HasValue || workAddressId.HasValue;
        if (dataSourceId == 1 && !string.IsNullOrEmpty(dto.IdentityNumber) && !hasAddressFromForm)
        {
            var addressData = await _absherService.GetNationalAddressAsync(dto.IdentityNumber, cancellationToken);
            if (addressData != null)
            {
                bool absherAddressesCreated = false;

                // Create residence address
                if (!string.IsNullOrEmpty(addressData.ResidenceCity))
                {
                    var absherResidenceAddress = await _addressRepository.CreateFromAbsherDataAsync(
                        addressData.ResidenceBuildingNumber ?? "",
                        addressData.ResidenceStreetName ?? "",
                        addressData.ResidenceDistrict ?? "",
                        addressData.ResidenceCity,
                        addressData.ResidencePostalCode ?? "",
                        addressData.ResidenceAdditionalNumber ?? "",
                        "Residence",
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    plaintiff.ResidenceAddressId = absherResidenceAddress.Id;
                    absherAddressesCreated = true;
                }

                // Create work address
                if (!string.IsNullOrEmpty(addressData.WorkCity))
                {
                    var absherWorkAddress = await _addressRepository.CreateFromAbsherDataAsync(
                        addressData.WorkBuildingNumber ?? "",
                        addressData.WorkStreetName ?? "",
                        addressData.WorkDistrict ?? "",
                        addressData.WorkCity,
                        addressData.WorkPostalCode ?? "",
                        addressData.WorkAdditionalNumber ?? "",
                        "Work",
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    plaintiff.WorkAddressId = absherWorkAddress.Id;
                    absherAddressesCreated = true;
                }

                if (absherAddressesCreated)
                {
                    await _plaintiffRepository.UpdateAsync(plaintiff, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        // Create representatives if provided
        if (dto.Representatives?.Any() == true)
        {
            _logger.LogInformation("Creating {Count} representatives for plaintiff {PlaintiffId}", dto.Representatives.Count, plaintiff.Id);

            foreach (var repDto in dto.Representatives)
            {
                var representative = new Representative
                {
                    PlaintiffId = plaintiff.Id,
                    RepresentativeTypeId = repDto.RepresentativeTypeId,
                    IdentityTypeId = repDto.IdentityTypeId,
                    IdentityNumber = repDto.IdentityNumber,
                    FirstName = repDto.FirstName,
                    FatherName = repDto.FatherName,
                    GrandfatherName = repDto.GrandfatherName,
                    FamilyName = repDto.FamilyName,
                    ClanName = repDto.ClanName,
                    BirthDate = repDto.BirthDate,
                    Gender = repDto.Gender,
                    NationalityId = repDto.NationalityId,
                    IdentityIssueDate = repDto.IdentityIssueDate,
                    IdentityExpiryDate = repDto.IdentityExpiryDate,
                    MobileNumber = repDto.MobileNumber,
                    Email = repDto.Email,
                    AuthorizationNumber = repDto.AuthorizationNumber,
                    AuthorizationDate = repDto.AuthorizationDate,
                    AuthorizationSource = repDto.AuthorizationSource,
                    AuthorizationSourceType = repDto.AuthorizationSourceType,
                    GuardianshipType = repDto.GuardianshipType,
                    DataSourceId = 2, // FromUser
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _representativeRepository.AddAsync(representative, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully created {Count} representatives", dto.Representatives.Count);
        }

        return await GetByIdAsync(plaintiff.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to create plaintiff");
    }

    public async Task<PlaintiffVM> UpdateAsync(int id, PlaintiffUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== UpdateAsync START for ID: {Id} ===", id);
        _logger.LogInformation("DTO received - FirstName: {FirstName}, FatherName: {FatherName}, MobileNumber: {Mobile}, Gender: {Gender}",
            dto?.FirstName, dto?.FatherName, dto?.MobileNumber, dto?.Gender);

        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Use GetByIdAsync (tracked) for updates, not GetWithDetailsAsync (untracked)
        var plaintiff = await _plaintiffRepository.GetByIdAsync(id, cancellationToken) as Plaintiff;
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {id} غير موجود");

        _logger.LogInformation("BEFORE update - FirstName: {FirstName}, FatherName: {FatherName}, MobileNumber: {Mobile}, DataSourceId: {DataSource}",
            plaintiff.FirstName, plaintiff.FatherName, plaintiff.MobileNumber, plaintiff.DataSourceId);

        // Update PlaintiffTypeId if provided (always allowed)
        if (dto.PlaintiffTypeId.HasValue)
        {
            _logger.LogInformation("Changing PlaintiffTypeId from {Old} to {New}", plaintiff.PlaintiffTypeId, dto.PlaintiffTypeId.Value);
            plaintiff.PlaintiffTypeId = dto.PlaintiffTypeId.Value;

            // Clear type-specific fields when changing plaintiff type
            var newTypeId = dto.PlaintiffTypeId.Value;

            // Clear Government Agency fields if not Type 6
            if (newTypeId != 6)
            {
                plaintiff.GovernmentAgencyId = null;
                plaintiff.Headquarters = null;
            }

            // Clear Waqf fields if not Type 8
            if (newTypeId != 8)
            {
                plaintiff.WaqfName = null;
                plaintiff.WaqfAddressId = null;
                plaintiff.WaqfAgencyName = null;
                plaintiff.WaqfDescription = null;
                plaintiff.WaqfOversightType = null;
                plaintiff.CourtDeedNumber = null;
                plaintiff.DeedDate = null;
                plaintiff.DeedSource = null;
            }

            // Clear NGO fields if not Type 7
            if (newTypeId != 7)
            {
                plaintiff.NGOName = null;
                plaintiff.NGOAddressId = null;
                plaintiff.LicenseNumber = null;
                plaintiff.LicenseDate = null;
                plaintiff.LicenseSourceId = null;
            }

            // Clear Company fields if not Type 4 or 5
            if (newTypeId != 4 && newTypeId != 5)
            {
                plaintiff.CompanyName = null;
                plaintiff.CompanyAddressId = null;
                plaintiff.UnregisteredCompanyAddress = null;
                plaintiff.UnregisteredCompanyCity = null;
            }

            // Clear Registered Company fields if not Type 4
            if (newTypeId != 4)
            {
                plaintiff.CommercialRegNumber = null;
                plaintiff.CRStartDate = null;
                plaintiff.CREndDate = null;
            }

            // Clear Business Owner fields if not Type 3
            if (newTypeId != 3)
            {
                plaintiff.BusinessAddressId = null;
            }

            // Clear Individual fields if not Type 1 or 2
            if (newTypeId != 1 && newTypeId != 2)
            {
                plaintiff.EmploymentStatusId = null;
                plaintiff.Employer = null;
                plaintiff.Profession = null;
                plaintiff.WorkAddressId = null;
            }
        }

        // Update fields (only non-Absher fields can be updated if data is from Absher)
        bool isFromAbsher = plaintiff.DataSourceId == 1;
        _logger.LogInformation("IsFromAbsher: {IsFromAbsher}", isFromAbsher);

        if (!isFromAbsher) // Not from Absher - allow identity and name updates
        {
            // Identity data
            if (dto.IdentityTypeId.HasValue) plaintiff.IdentityTypeId = dto.IdentityTypeId;
            if (dto.IdentityNumber != null) plaintiff.IdentityNumber = dto.IdentityNumber;
            if (dto.IdentityIssueDate.HasValue) plaintiff.IdentityIssueDate = dto.IdentityIssueDate;
            if (dto.IdentityExpiryDate.HasValue) plaintiff.IdentityExpiryDate = dto.IdentityExpiryDate;

            // Personal data
            if (dto.FirstName != null) plaintiff.FirstName = dto.FirstName;
            if (dto.FatherName != null) plaintiff.FatherName = dto.FatherName;
            if (dto.GrandfatherName != null) plaintiff.GrandfatherName = dto.GrandfatherName;
            if (dto.ClanName != null) plaintiff.ClanName = dto.ClanName;
            if (dto.FamilyName != null) plaintiff.FamilyName = dto.FamilyName;
            if (dto.Gender != null) plaintiff.Gender = dto.Gender;
            if (dto.BirthDate.HasValue) plaintiff.BirthDate = dto.BirthDate;
            if (dto.NationalityId.HasValue) plaintiff.NationalityId = dto.NationalityId;
        }

        // Document number (for Type 2 - can be updated if not readonly)
        if (dto.DocumentNumber != null) plaintiff.DocumentNumber = dto.DocumentNumber;

        // Contact info can always be updated
        if (dto.MobileNumber != null) plaintiff.MobileNumber = dto.MobileNumber;
        if (dto.Email != null) plaintiff.Email = dto.Email;

        // Employment data
        if (dto.EmploymentStatusId.HasValue) plaintiff.EmploymentStatusId = dto.EmploymentStatusId;
        if (dto.Employer != null) plaintiff.Employer = dto.Employer;
        if (dto.Profession != null) plaintiff.Profession = dto.Profession;

        // Business/Company data
        if (dto.CompanyName != null) plaintiff.CompanyName = dto.CompanyName;
        if (dto.CommercialRegNumber != null) plaintiff.CommercialRegNumber = dto.CommercialRegNumber;
        if (dto.CRStartDate.HasValue) plaintiff.CRStartDate = dto.CRStartDate;
        if (dto.CREndDate.HasValue) plaintiff.CREndDate = dto.CREndDate;

        // Unregistered Company (Type 5) specific fields
        if (dto.CountryId.HasValue) plaintiff.CountryId = dto.CountryId > 0 ? dto.CountryId : null;
        if (dto.UnregisteredCompanyCity != null) plaintiff.UnregisteredCompanyCity = dto.UnregisteredCompanyCity;
        if (dto.Description != null) plaintiff.Description = dto.Description;

        if (dto.LicenseNumber != null) plaintiff.LicenseNumber = dto.LicenseNumber;
        if (dto.LicenseSourceId.HasValue) plaintiff.LicenseSourceId = dto.LicenseSourceId > 0 ? dto.LicenseSourceId : null;
        if (dto.NGOName != null) plaintiff.NGOName = dto.NGOName;
        if (dto.LicenseDate.HasValue) plaintiff.LicenseDate = dto.LicenseDate;
        // Treat GovernmentAgencyId = 0 as null to avoid FK constraint errors
        if (dto.GovernmentAgencyId.HasValue) plaintiff.GovernmentAgencyId = dto.GovernmentAgencyId > 0 ? dto.GovernmentAgencyId : null;
        if (dto.Headquarters != null) plaintiff.Headquarters = dto.Headquarters;
        if (dto.WaqfOversightType != null) plaintiff.WaqfOversightType = dto.WaqfOversightType;

        // Additional info
        if (dto.AdditionalStatement != null) plaintiff.AdditionalStatement = dto.AdditionalStatement;

        // Handle IsApplicant - IMPORTANT: Don't return early, save all changes first
        bool needToSetAsApplicant = dto.IsApplicant.HasValue && dto.IsApplicant.Value && !plaintiff.IsApplicant;

        if (dto.IsApplicant.HasValue)
        {
            plaintiff.IsApplicant = dto.IsApplicant.Value;
        }

        // Update Residence Address
        if (dto.ResidenceAddress != null)
        {
            if (plaintiff.ResidenceAddressId.HasValue)
            {
                // Load and update existing address
                var existingAddress = await _addressRepository.GetByIdAsync(plaintiff.ResidenceAddressId.Value, cancellationToken) as Address;
                if (existingAddress != null)
                {
                    UpdateAddress(existingAddress, dto.ResidenceAddress);
                    await _addressRepository.UpdateAsync(existingAddress, cancellationToken);
                }
            }
            else
            {
                // Create new address
                var address = CreateAddressFromDTO(dto.ResidenceAddress, "Residence");
                await _addressRepository.AddAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                plaintiff.ResidenceAddressId = address.Id;
            }
        }

        // Update Work Address
        if (dto.WorkAddress != null)
        {
            if (plaintiff.WorkAddressId.HasValue)
            {
                // Load and update existing address
                var existingAddress = await _addressRepository.GetByIdAsync(plaintiff.WorkAddressId.Value, cancellationToken) as Address;
                if (existingAddress != null)
                {
                    UpdateAddress(existingAddress, dto.WorkAddress);
                    await _addressRepository.UpdateAsync(existingAddress, cancellationToken);
                }
            }
            else
            {
                // Create new address
                var address = CreateAddressFromDTO(dto.WorkAddress, "Work");
                await _addressRepository.AddAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                plaintiff.WorkAddressId = address.Id;
            }
        }

        // Update Business Address (for Business Owner - Type 6)
        if (dto.BusinessAddress != null && dto.BusinessAddress.RegionId > 0 && dto.BusinessAddress.CityId > 0)
        {
            if (plaintiff.BusinessAddressId.HasValue)
            {
                var existingAddress = await _addressRepository.GetByIdAsync(plaintiff.BusinessAddressId.Value, cancellationToken) as Address;
                if (existingAddress != null)
                {
                    UpdateAddress(existingAddress, dto.BusinessAddress);
                    await _addressRepository.UpdateAsync(existingAddress, cancellationToken);
                }
            }
            else
            {
                var address = CreateAddressFromDTO(dto.BusinessAddress, "Business");
                await _addressRepository.AddAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                plaintiff.BusinessAddressId = address.Id;
            }
        }

        // Update Company Address (for Registered Company - Type 2)
        if (dto.CompanyAddress != null && dto.CompanyAddress.RegionId > 0 && dto.CompanyAddress.CityId > 0)
        {
            if (plaintiff.CompanyAddressId.HasValue)
            {
                var existingAddress = await _addressRepository.GetByIdAsync(plaintiff.CompanyAddressId.Value, cancellationToken) as Address;
                if (existingAddress != null)
                {
                    UpdateAddress(existingAddress, dto.CompanyAddress);
                    await _addressRepository.UpdateAsync(existingAddress, cancellationToken);
                }
            }
            else
            {
                var address = CreateAddressFromDTO(dto.CompanyAddress, "Company");
                await _addressRepository.AddAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                plaintiff.CompanyAddressId = address.Id;
            }
        }

        // Update NGO Address (for NGO - Type 4)
        if (dto.NGOAddress != null && dto.NGOAddress.RegionId > 0 && dto.NGOAddress.CityId > 0)
        {
            if (plaintiff.NGOAddressId.HasValue)
            {
                var existingAddress = await _addressRepository.GetByIdAsync(plaintiff.NGOAddressId.Value, cancellationToken) as Address;
                if (existingAddress != null)
                {
                    UpdateAddress(existingAddress, dto.NGOAddress);
                    await _addressRepository.UpdateAsync(existingAddress, cancellationToken);
                }
            }
            else
            {
                var address = CreateAddressFromDTO(dto.NGOAddress, "NGO");
                await _addressRepository.AddAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                plaintiff.NGOAddressId = address.Id;
            }
        }

        // Update Waqf Address (for Waqf - Type 5)
        if (dto.WaqfAddress != null && dto.WaqfAddress.RegionId > 0 && dto.WaqfAddress.CityId > 0)
        {
            if (plaintiff.WaqfAddressId.HasValue)
            {
                var existingAddress = await _addressRepository.GetByIdAsync(plaintiff.WaqfAddressId.Value, cancellationToken) as Address;
                if (existingAddress != null)
                {
                    UpdateAddress(existingAddress, dto.WaqfAddress);
                    await _addressRepository.UpdateAsync(existingAddress, cancellationToken);
                }
            }
            else
            {
                var address = CreateAddressFromDTO(dto.WaqfAddress, "Waqf");
                await _addressRepository.AddAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                plaintiff.WaqfAddressId = address.Id;
            }
        }

        // Update Selected Address (العنوان المختار)
        if (dto.SelectedAddress != null && dto.SelectedAddress.RegionId > 0 && dto.SelectedAddress.CityId > 0)
        {
            if (plaintiff.SelectedAddressId.HasValue)
            {
                var existingAddress = await _addressRepository.GetByIdAsync(plaintiff.SelectedAddressId.Value, cancellationToken) as Address;
                if (existingAddress != null)
                {
                    UpdateAddress(existingAddress, dto.SelectedAddress);
                    await _addressRepository.UpdateAsync(existingAddress, cancellationToken);
                }
            }
            else
            {
                var address = CreateAddressFromDTO(dto.SelectedAddress, "Selected");
                await _addressRepository.AddAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                plaintiff.SelectedAddressId = address.Id;
            }
        }

        plaintiff.ModifiedDate = DateTime.UtcNow;

        _logger.LogInformation("AFTER update - FirstName: {FirstName}, FatherName: {FatherName}, MobileNumber: {Mobile}",
            plaintiff.FirstName, plaintiff.FatherName, plaintiff.MobileNumber);

        await _plaintiffRepository.UpdateAsync(plaintiff, cancellationToken);
        _logger.LogInformation("Called UpdateAsync on repository");

        var rowsAffected = await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("SaveChangesAsync completed - Rows affected: {RowsAffected}", rowsAffected);

        // If setting as applicant, clear other applicants (after our changes are saved)
        if (needToSetAsApplicant)
        {
            _logger.LogInformation("Setting plaintiff {Id} as applicant - clearing others", id);
            await ClearOtherApplicantsAsync(id, cancellationToken);
        }

        var result = await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to update plaintiff");
        _logger.LogInformation("=== UpdateAsync END - Returning FirstName: {FirstName} ===", result.FirstName);

        return result;
    }

    /// <summary>
    /// Clears IsApplicant flag from all other plaintiffs in the same case request.
    /// </summary>
    private async Task ClearOtherApplicantsAsync(int currentPlaintiffId, CancellationToken cancellationToken)
    {
        // Get request ID from junction table
        var associations = await _caseRequestPlaintiffRepository.GetByPlaintiffIdAsync(currentPlaintiffId, cancellationToken);
        var requestId = associations.FirstOrDefault()?.CaseRegistrationRequestId;

        if (requestId.HasValue)
        {
            // Clear previous applicants
            var requestPlaintiffs = await _plaintiffRepository.GetByRequestIdAsync(requestId.Value, cancellationToken);
            foreach (var p in requestPlaintiffs.Where(p => p.IsApplicant && p.Id != currentPlaintiffId))
            {
                p.IsApplicant = false;
                p.ModifiedDate = DateTime.UtcNow;
                await _plaintiffRepository.UpdateAsync(p, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var plaintiff = await _plaintiffRepository.GetByIdAsync(id, cancellationToken) as Plaintiff;
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {id} غير موجود");

        // Soft delete
        plaintiff.IsDeleted = true;
        plaintiff.IsActive = false;
        plaintiff.ModifiedDate = DateTime.UtcNow;

        await _plaintiffRepository.UpdateAsync(plaintiff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlaintiffVM> SetAsApplicantAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        var plaintiff = await _plaintiffRepository.GetWithDetailsAsync(plaintiffId, cancellationToken);
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {plaintiffId} غير موجود");

        // BC07: Only Individual can be applicant
        if (plaintiff.PlaintiffTypeId != PlaintiffTypeIndividual && plaintiff.PlaintiffTypeId != PlaintiffTypeIndividualNoId)
            throw new InvalidOperationException("BC07: فقط المدعي من نوع 'فرد' يمكنه أن يكون مقدم الطلب");

        // Get request ID from junction table
        var associations = await _caseRequestPlaintiffRepository.GetByPlaintiffIdAsync(plaintiffId, cancellationToken);
        var requestId = associations.FirstOrDefault()?.CaseRegistrationRequestId;

        if (requestId.HasValue)
        {
            // Clear previous applicant
            var requestPlaintiffs = await _plaintiffRepository.GetByRequestIdAsync(requestId.Value, cancellationToken);
            foreach (var p in requestPlaintiffs.Where(p => p.IsApplicant && p.Id != plaintiffId))
            {
                p.IsApplicant = false;
                p.ModifiedDate = DateTime.UtcNow;
                await _plaintiffRepository.UpdateAsync(p, cancellationToken);
            }
        }

        // Set as applicant
        plaintiff.IsApplicant = true;
        plaintiff.ModifiedDate = DateTime.UtcNow;

        await _plaintiffRepository.UpdateAsync(plaintiff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(plaintiffId, cancellationToken) ?? throw new InvalidOperationException("Failed to set applicant");
    }

    public async Task<AddressVM?> GetSelectedAddressAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        var plaintiff = await _plaintiffRepository.GetWithDetailsAsync(plaintiffId, cancellationToken);
        if (plaintiff?.SelectedAddress == null)
            return null;

        return MapToAddressVM(plaintiff.SelectedAddress);
    }

    public async Task<AddressVM> SetSelectedAddressAsync(int plaintiffId, SelectedAddressDTO dto, CancellationToken cancellationToken = default)
    {
        var plaintiff = await _plaintiffRepository.GetWithDetailsAsync(plaintiffId, cancellationToken);
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {plaintiffId} غير موجود");

        int? selectedAddressId = null;

        if (dto.CustomAddress != null)
        {
            // Create custom address
            var address = new Address
            {
                BuildingNumber = dto.CustomAddress.BuildingNumber,
                StreetName = dto.CustomAddress.StreetName,
                District = dto.CustomAddress.District,
                City = dto.CustomAddress.City,
                CityId = dto.CustomAddress.CityId,
                RegionId = dto.CustomAddress.RegionId,
                PostalCode = dto.CustomAddress.PostalCode,
                AdditionalNumber = dto.CustomAddress.AdditionalNumber,
                UnitNumber = dto.CustomAddress.UnitNumber,
                AddressType = "Selected",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _addressRepository.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            selectedAddressId = address.Id;
        }
        else
        {
            // Use existing address based on type
            selectedAddressId = dto.AddressSourceType == "Residence"
                ? plaintiff.ResidenceAddressId
                : plaintiff.WorkAddressId;

            if (!selectedAddressId.HasValue)
                throw new InvalidOperationException($"العنوان المطلوب ({dto.AddressSourceType}) غير متوفر");
        }

        plaintiff.SelectedAddressId = selectedAddressId;
        plaintiff.ModifiedDate = DateTime.UtcNow;

        await _plaintiffRepository.UpdateAsync(plaintiff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetSelectedAddressAsync(plaintiffId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to set selected address");
    }

    #region Mapping Helpers

    private static PlaintiffListVM MapToListVM(Plaintiff plaintiff)
    {
        var displayName = plaintiff.PlaintiffTypeId switch
        {
            3 or 4 or 5 => plaintiff.CompanyName ?? "",
            6 => plaintiff.GovernmentAgency?.NameAr ?? "",
            _ => string.Join(" ", new[] { plaintiff.FirstName, plaintiff.FatherName, plaintiff.GrandfatherName, plaintiff.FamilyName }
                .Where(n => !string.IsNullOrWhiteSpace(n)))
        };

        return new PlaintiffListVM
        {
            Id = plaintiff.Id,
            PlaintiffTypeId = plaintiff.PlaintiffTypeId,
            PlaintiffTypeNameAr = plaintiff.PlaintiffType?.NameAr ?? "",
            DisplayName = displayName,
            IdentityNumber = plaintiff.IdentityNumber,
            MobileNumber = plaintiff.MobileNumber,
            IsApplicant = plaintiff.IsApplicant,
            DataSourceId = plaintiff.DataSourceId,
            DataSourceName = plaintiff.DataSource?.NameAr,
            RepresentativesCount = plaintiff.Representatives?.Count(r => !r.IsDeleted && r.IsActive) ?? 0,
            AttachmentsCount = plaintiff.Attachments?.Count(a => !a.IsDeleted && a.IsActive) ?? 0
        };
    }

    private static PlaintiffVM MapToVM(Plaintiff plaintiff)
    {
        return new PlaintiffVM
        {
            Id = plaintiff.Id,
            PlaintiffTypeId = plaintiff.PlaintiffTypeId,
            PlaintiffTypeName = plaintiff.PlaintiffType?.Name ?? "",
            PlaintiffTypeNameAr = plaintiff.PlaintiffType?.NameAr ?? "",
            // Personal data - include IDs for editing
            IdentityTypeId = plaintiff.IdentityTypeId,
            IdentityTypeName = plaintiff.IdentityType?.NameAr,
            IdentityNumber = plaintiff.IdentityNumber,
            FirstName = plaintiff.FirstName,
            FatherName = plaintiff.FatherName,
            GrandfatherName = plaintiff.GrandfatherName,
            ClanName = plaintiff.ClanName,
            FamilyName = plaintiff.FamilyName,
            BirthDate = plaintiff.BirthDate,
            Gender = plaintiff.Gender,
            NationalityId = plaintiff.NationalityId,
            IdentityIssueDate = plaintiff.IdentityIssueDate,
            IdentityExpiryDate = plaintiff.IdentityExpiryDate,
            DocumentNumber = plaintiff.DocumentNumber,
            // Data source
            DataSourceId = plaintiff.DataSourceId,
            DataSourceName = plaintiff.DataSource?.NameAr,
            // Contact
            MobileNumber = plaintiff.MobileNumber,
            Email = plaintiff.Email,
            // Employment - include ID for editing
            EmploymentStatusId = plaintiff.EmploymentStatusId,
            Employer = plaintiff.Employer,
            Profession = plaintiff.Profession,
            // Business/Company
            CommercialRegNumber = plaintiff.CommercialRegNumber,
            CompanyName = plaintiff.CompanyName,
            CRStartDate = plaintiff.CRStartDate,
            CREndDate = plaintiff.CREndDate,
            // Unregistered Company - include ID for editing
            UnregisteredCompanyAddress = plaintiff.UnregisteredCompanyAddress,
            CountryId = plaintiff.CountryId,
            CountryName = plaintiff.Country?.NameAr,
            UnregisteredCompanyCity = plaintiff.UnregisteredCompanyCity,
            Description = plaintiff.Description,
            // Government Agency - include ID for editing
            GovernmentAgencyId = plaintiff.GovernmentAgencyId,
            GovernmentAgencyName = plaintiff.GovernmentAgency?.NameAr,
            Headquarters = plaintiff.Headquarters,
            AdditionalStatement = plaintiff.AdditionalStatement,
            // NGO - include ID for editing
            LicenseNumber = plaintiff.LicenseNumber,
            LicenseSourceId = plaintiff.LicenseSourceId,
            LicenseSourceName = plaintiff.LicenseSource?.NameAr,
            NGOName = plaintiff.NGOName,
            LicenseDate = plaintiff.LicenseDate,
            // Waqf
            CourtDeedNumber = plaintiff.CourtDeedNumber,
            WaqfName = plaintiff.WaqfName,
            DeedDate = plaintiff.DeedDate,
            DeedSource = plaintiff.DeedSource,
            WaqfOversightType = plaintiff.WaqfOversightType,
            WaqfAgencyName = plaintiff.WaqfAgencyName,
            WaqfDescription = plaintiff.WaqfDescription,
            // Status
            IsApplicant = plaintiff.IsApplicant,
            CreatedDate = plaintiff.CreatedDate,
            // Addresses
            ResidenceAddress = plaintiff.ResidenceAddress != null ? MapToAddressVM(plaintiff.ResidenceAddress) : null,
            WorkAddress = plaintiff.WorkAddress != null ? MapToAddressVM(plaintiff.WorkAddress) : null,
            BusinessAddress = plaintiff.BusinessAddress != null ? MapToAddressVM(plaintiff.BusinessAddress) : null,
            CompanyAddress = plaintiff.CompanyAddress != null ? MapToAddressVM(plaintiff.CompanyAddress) : null,
            NGOAddress = plaintiff.NGOAddress != null ? MapToAddressVM(plaintiff.NGOAddress) : null,
            WaqfAddress = plaintiff.WaqfAddress != null ? MapToAddressVM(plaintiff.WaqfAddress) : null,
            SelectedAddress = plaintiff.SelectedAddress != null ? MapToAddressVM(plaintiff.SelectedAddress) : null,
            // Related data
            Representatives = plaintiff.Representatives?
                .Where(r => !r.IsDeleted && r.IsActive)
                .Select(MapToRepresentativeVM)
                .ToList() ?? new List<RepresentativeVM>(),
            Attachments = plaintiff.Attachments?
                .Where(a => !a.IsDeleted && a.IsActive)
                .Select(MapToAttachmentVM)
                .ToList() ?? new List<PlaintiffAttachmentVM>()
        };
    }

    private static Address CreateAddressFromDTO(AddressCreateDTO dto, string addressType)
    {
        return new Address
        {
            RegionId = dto.RegionId,
            CityId = dto.CityId,
            District = dto.DistrictId?.ToString(),
            StreetName = dto.Street,
            BuildingNumber = dto.BuildingNumber,
            UnitNumber = dto.UnitNumber,
            PostalCode = dto.PostalCode,
            AdditionalNumber = dto.AdditionalCode,
            AddressType = addressType,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
    }

    private static void UpdateAddress(Address address, AddressCreateDTO dto)
    {
        address.RegionId = dto.RegionId;
        address.CityId = dto.CityId;
        address.District = dto.DistrictId?.ToString();
        address.StreetName = dto.Street;
        address.BuildingNumber = dto.BuildingNumber;
        address.UnitNumber = dto.UnitNumber;
        address.PostalCode = dto.PostalCode;
        address.AdditionalNumber = dto.AdditionalCode;
        address.ModifiedDate = DateTime.UtcNow;
    }

    private static AddressVM MapToAddressVM(Address address)
    {
        return new AddressVM
        {
            Id = address.Id,
            BuildingNumber = address.BuildingNumber,
            StreetName = address.StreetName,
            District = address.District,
            City = address.City,
            CityId = address.CityId,
            CityName = address.City_?.NameAr,
            RegionId = address.RegionId,
            RegionName = address.Region?.NameAr,
            DistrictId = int.TryParse(address.District, out var districtId) ? districtId : null,
            DistrictName = address.District, // District is stored as string name
            PostalCode = address.PostalCode,
            AdditionalNumber = address.AdditionalNumber,
            UnitNumber = address.UnitNumber,
            AddressType = address.AddressType
        };
    }

    private static RepresentativeVM MapToRepresentativeVM(Representative rep)
    {
        return new RepresentativeVM
        {
            Id = rep.Id,
            PlaintiffId = rep.PlaintiffId,
            RepresentativeTypeId = rep.RepresentativeTypeId,
            RepresentativeTypeNameAr = rep.RepresentativeType?.NameAr ?? "",
            RepresentativeTypeName = rep.RepresentativeType?.Name ?? "",
            IdentityTypeId = rep.IdentityTypeId,
            IdentityTypeName = rep.IdentityType?.NameAr ?? "",
            IdentityNumber = rep.IdentityNumber,
            FirstName = rep.FirstName,
            FatherName = rep.FatherName,
            GrandfatherName = rep.GrandfatherName,
            FamilyName = rep.FamilyName,
            ClanName = rep.ClanName,
            BirthDate = rep.BirthDate,
            Gender = rep.Gender,
            NationalityId = rep.NationalityId,
            IdentityIssueDate = rep.IdentityIssueDate,
            IdentityExpiryDate = rep.IdentityExpiryDate,
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

    private static PlaintiffAttachmentVM MapToAttachmentVM(PlaintiffAttachment attachment)
    {
        return new PlaintiffAttachmentVM
        {
            Id = attachment.Id,
            PlaintiffId = attachment.PlaintiffId,
            AttachmentTypeId = attachment.AttachmentTypeId,
            AttachmentTypeNameAr = attachment.AttachmentType?.NameAr ?? "",
            AttachmentTypeName = attachment.AttachmentType?.Name ?? "",
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSizeBytes = attachment.FileSizeBytes,
            Description = attachment.Description,
            UploadDate = attachment.UploadDate
        };
    }

    #endregion
}
