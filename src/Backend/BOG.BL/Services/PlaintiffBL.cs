using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Common;
using BOG.DTO.Plaintiff;
using BOG.Integration.Interfaces;
using BOG.VM.Common;
using BOG.VM.Plaintiff;
using BOG.VM.Representative;

namespace BOG.BL.Services;

/// <summary>
/// Plaintiff business logic service.
/// Handles plaintiff CRUD operations and business rules.
/// </summary>
public class PlaintiffBL : IPlaintiffBL
{
    private readonly IPlaintiffRepository _plaintiffRepository;
    private readonly ICaseRequestPlaintiffRepository _caseRequestPlaintiffRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IAbsherService _absherService;
    private readonly IUnitOfWork _unitOfWork;

    // Plaintiff type constants
    private const int PlaintiffTypeIndividual = 1;
    private const int PlaintiffTypeIndividualNoId = 2;

    public PlaintiffBL(
        IPlaintiffRepository plaintiffRepository,
        ICaseRequestPlaintiffRepository caseRequestPlaintiffRepository,
        IAddressRepository addressRepository,
        IAbsherService absherService,
        IUnitOfWork unitOfWork)
    {
        _plaintiffRepository = plaintiffRepository ?? throw new ArgumentNullException(nameof(plaintiffRepository));
        _caseRequestPlaintiffRepository = caseRequestPlaintiffRepository ?? throw new ArgumentNullException(nameof(caseRequestPlaintiffRepository));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _absherService = absherService ?? throw new ArgumentNullException(nameof(absherService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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

        // Create plaintiff entity
        var plaintiff = new Plaintiff
        {
            PlaintiffTypeId = dto.PlaintiffTypeId,
            IdentityTypeId = dto.IdentityTypeId,
            IdentityNumber = dto.IdentityNumber,
            FirstName = dto.FirstName,
            FatherName = dto.FatherName,
            GrandfatherName = dto.GrandfatherName,
            FamilyName = dto.FamilyName,
            BirthDate = dto.BirthDate,
            Gender = dto.Gender,
            NationalityId = dto.NationalityId,
            DataSourceId = dataSourceId,
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            CommercialRegNumber = dto.CommercialRegNumber,
            CompanyName = dto.CompanyName,
            GovernmentAgencyId = dto.GovernmentAgencyId,
            AdditionalStatement = dto.AdditionalStatement,
            LicenseNumber = dto.LicenseNumber,
            LicenseSource = dto.LicenseSource,
            LicenseDate = dto.LicenseDate,
            CourtDeedNumber = dto.CourtDeedNumber,
            DeedDate = dto.DeedDate,
            DeedSource = dto.DeedSource,
            WaqfOversightType = dto.WaqfOversightType,
            IsApplicant = false,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _plaintiffRepository.AddAsync(plaintiff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create junction table entry
        var caseRequestPlaintiff = new CaseRequestPlaintiff
        {
            CaseRegistrationRequestId = requestId,
            PlaintiffId = plaintiff.Id,
            CreatedDate = DateTime.UtcNow
        };

        await _caseRequestPlaintiffRepository.AddAsync(caseRequestPlaintiff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch addresses from Absher if available
        if (dataSourceId == 1 && !string.IsNullOrEmpty(dto.IdentityNumber))
        {
            var addressData = await _absherService.GetNationalAddressAsync(dto.IdentityNumber, cancellationToken);
            if (addressData != null)
            {
                // Create residence address
                if (!string.IsNullOrEmpty(addressData.ResidenceCity))
                {
                    var residenceAddress = await _addressRepository.CreateFromAbsherDataAsync(
                        addressData.ResidenceBuildingNumber ?? "",
                        addressData.ResidenceStreetName ?? "",
                        addressData.ResidenceDistrict ?? "",
                        addressData.ResidenceCity,
                        addressData.ResidencePostalCode ?? "",
                        addressData.ResidenceAdditionalNumber ?? "",
                        "Residence",
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    plaintiff.ResidenceAddressId = residenceAddress.Id;
                }

                // Create work address
                if (!string.IsNullOrEmpty(addressData.WorkCity))
                {
                    var workAddress = await _addressRepository.CreateFromAbsherDataAsync(
                        addressData.WorkBuildingNumber ?? "",
                        addressData.WorkStreetName ?? "",
                        addressData.WorkDistrict ?? "",
                        addressData.WorkCity,
                        addressData.WorkPostalCode ?? "",
                        addressData.WorkAdditionalNumber ?? "",
                        "Work",
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    plaintiff.WorkAddressId = workAddress.Id;
                }

                await _plaintiffRepository.UpdateAsync(plaintiff, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return await GetByIdAsync(plaintiff.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to create plaintiff");
    }

    public async Task<PlaintiffVM> UpdateAsync(int id, PlaintiffUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var plaintiff = await _plaintiffRepository.GetByIdAsync(id, cancellationToken) as Plaintiff;
        if (plaintiff == null || plaintiff.IsDeleted)
            throw new InvalidOperationException($"المدعي رقم {id} غير موجود");

        // Update fields (only non-Absher fields can be updated if data is from Absher)
        if (plaintiff.DataSourceId != 1) // Not from Absher - allow name updates
        {
            if (!string.IsNullOrEmpty(dto.FirstName)) plaintiff.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.FatherName)) plaintiff.FatherName = dto.FatherName;
            if (!string.IsNullOrEmpty(dto.GrandfatherName)) plaintiff.GrandfatherName = dto.GrandfatherName;
            if (!string.IsNullOrEmpty(dto.FamilyName)) plaintiff.FamilyName = dto.FamilyName;
        }

        // Contact info can always be updated
        if (dto.MobileNumber != null) plaintiff.MobileNumber = dto.MobileNumber;
        if (dto.Email != null) plaintiff.Email = dto.Email;

        // Other fields
        if (dto.CompanyName != null) plaintiff.CompanyName = dto.CompanyName;
        if (dto.AdditionalStatement != null) plaintiff.AdditionalStatement = dto.AdditionalStatement;
        if (dto.Employer != null) plaintiff.Employer = dto.Employer;
        if (dto.Profession != null) plaintiff.Profession = dto.Profession;

        plaintiff.ModifiedDate = DateTime.UtcNow;

        await _plaintiffRepository.UpdateAsync(plaintiff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to update plaintiff");
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
            FirstName = plaintiff.FirstName,
            FatherName = plaintiff.FatherName,
            GrandfatherName = plaintiff.GrandfatherName,
            FamilyName = plaintiff.FamilyName,
            IdentityTypeName = plaintiff.IdentityType?.NameAr,
            IdentityNumber = plaintiff.IdentityNumber,
            BirthDate = plaintiff.BirthDate,
            Gender = plaintiff.Gender,
            DataSourceId = plaintiff.DataSourceId,
            DataSourceName = plaintiff.DataSource?.NameAr,
            MobileNumber = plaintiff.MobileNumber,
            Email = plaintiff.Email,
            CommercialRegNumber = plaintiff.CommercialRegNumber,
            CompanyName = plaintiff.CompanyName,
            GovernmentAgencyName = plaintiff.GovernmentAgency?.NameAr,
            AdditionalStatement = plaintiff.AdditionalStatement,
            IsApplicant = plaintiff.IsApplicant,
            CreatedDate = plaintiff.CreatedDate,
            ResidenceAddress = plaintiff.ResidenceAddress != null ? MapToAddressVM(plaintiff.ResidenceAddress) : null,
            WorkAddress = plaintiff.WorkAddress != null ? MapToAddressVM(plaintiff.WorkAddress) : null,
            SelectedAddress = plaintiff.SelectedAddress != null ? MapToAddressVM(plaintiff.SelectedAddress) : null,
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
            RegionId = address.RegionId,
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
