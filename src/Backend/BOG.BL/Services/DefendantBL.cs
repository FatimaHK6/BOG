using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Enums;
using BOG.DTO.Defendant;
using BOG.VM.Defendant;
using Microsoft.Extensions.Logging;

namespace BOG.BL.Services;

/// <summary>
/// Defendant business logic service.
/// </summary>
public class DefendantBL : IDefendantBL
{
    private readonly IDefendantRepository _defendantRepository;
    private readonly ICaseRequestDefendantRepository _caseRequestDefendantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DefendantBL> _logger;

    // Defendant type constants
    private const int TypeIndividual = 1;
    private const int TypeRegisteredCompany = 2;
    private const int TypeGovernmentAgency = 3;
    private const int TypeUnregisteredCompany = 4;
    private const int TypeNGO = 6;
    private const int TypeWaqf = 7;

    public DefendantBL(
        IDefendantRepository defendantRepository,
        ICaseRequestDefendantRepository caseRequestDefendantRepository,
        IUnitOfWork unitOfWork,
        ILogger<DefendantBL> logger)
    {
        _defendantRepository = defendantRepository ?? throw new ArgumentNullException(nameof(defendantRepository));
        _caseRequestDefendantRepository = caseRequestDefendantRepository ?? throw new ArgumentNullException(nameof(caseRequestDefendantRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<DefendantListVM>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var defendants = await _defendantRepository.GetByRequestIdAsync(requestId, cancellationToken);
        return defendants.Select(MapToListVM).ToList();
    }

    public async Task<DefendantVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var defendant = await _defendantRepository.GetWithDetailsAsync(id, cancellationToken);
        return defendant == null ? null : MapToVM(defendant);
    }

    public async Task<DefendantVM> CreateAsync(int requestId, DefendantCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        _logger.LogInformation("Creating defendant for request {RequestId}, type {TypeId}", requestId, dto.DefendantTypeId);

        // Check for duplicate defendant (same identity number and type on same request)
        // Applies to types that have identity numbers: Individual (1) and Business Owner (5)
        if ((dto.DefendantTypeId == TypeIndividual || dto.DefendantTypeId == 5) && !string.IsNullOrEmpty(dto.IdentityNumber))
        {
            var isDuplicate = await _defendantRepository.ExistsDuplicateAsync(requestId, dto.IdentityNumber, dto.DefendantTypeId, cancellationToken);
            if (isDuplicate)
            {
                _logger.LogWarning("Duplicate defendant detected: RequestId={RequestId}, IdentityNumber={IdentityNumber}, TypeId={TypeId}",
                    requestId, dto.IdentityNumber, dto.DefendantTypeId);
                throw new InvalidOperationException("يوجد مدعى عليه بنفس رقم الهوية ونوع المدعى عليه مسبقاً في هذا الطلب");
            }
        }

        // Build display name based on type
        string fullName = dto.DefendantTypeId switch
        {
            TypeIndividual => BuildIndividualFullName(dto.FirstName, dto.FatherName, dto.GrandfatherName, dto.TribeName, dto.FamilyName),
            TypeRegisteredCompany => dto.CompanyName ?? "شركة مسجلة",
            TypeGovernmentAgency => dto.Headquarters ?? "جهة حكومية",
            TypeUnregisteredCompany => dto.CompanyName ?? "شركة غير مسجلة",
            TypeNGO => dto.NGOName ?? "جمعية/مؤسسة أهلية",
            TypeWaqf => dto.WaqfName ?? "وقف",
            _ => "مدعى عليه"
        };

        // Create defendant entity
        var defendant = new Defendant
        {
            DefendantTypeId = dto.DefendantTypeId,
            FullName = fullName,
            // Individual fields (Type 1)
            IdentityTypeId = dto.IdentityTypeId,
            IdentityNumber = dto.IdentityNumber,
            FirstName = dto.FirstName,
            FatherName = dto.FatherName,
            GrandfatherName = dto.GrandfatherName,
            TribeName = dto.TribeName,
            FamilyName = dto.FamilyName,
            BirthDate = dto.BirthDate,
            IdentityIssueDate = dto.IdentityIssueDate,
            IdentityExpiryDate = dto.IdentityExpiryDate,
            GenderId = dto.GenderId,
            NationalityId = dto.NationalityId,
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            IndRegionId = dto.IndRegionId,
            IndCityId = dto.IndCityId,
            IndDistrict = dto.IndDistrict,
            IndStreet = dto.IndStreet,
            IndBuildingNumber = dto.IndBuildingNumber,
            IndUnitNumber = dto.IndUnitNumber,
            IndPostalCode = dto.IndPostalCode,
            IndAdditionalCode = dto.IndAdditionalCode,
            EmploymentStatusId = dto.EmploymentStatusId,
            Employer = dto.Employer,
            Occupation = dto.Occupation,
            // Work address fields (Type 1 - Private only)
            WorkRegionId = dto.WorkRegionId,
            WorkCityId = dto.WorkCityId,
            WorkDistrict = dto.WorkDistrict,
            WorkStreet = dto.WorkStreet,
            WorkBuildingNumber = dto.WorkBuildingNumber,
            WorkUnitNumber = dto.WorkUnitNumber,
            WorkPostalCode = dto.WorkPostalCode,
            WorkAdditionalCode = dto.WorkAdditionalCode,
            // Registered Company fields (Type 2) - uses shared CommercialRegNumber/CompanyName
            RegistrationStartDate = dto.RegistrationStartDate,
            RegistrationEndDate = dto.RegistrationEndDate,
            RegCompanyRegionId = dto.RegCompanyRegionId,
            RegCompanyCityId = dto.RegCompanyCityId,
            RegCompanyDistrict = dto.RegCompanyDistrict,
            RegCompanyStreet = dto.RegCompanyStreet,
            RegCompanyBuildingNumber = dto.RegCompanyBuildingNumber,
            RegCompanyUnitNumber = dto.RegCompanyUnitNumber,
            RegCompanyPostalCode = dto.RegCompanyPostalCode,
            RegCompanyAdditionalCode = dto.RegCompanyAdditionalCode,
            // Government Agency fields (Type 3)
            GovernmentAgencyId = dto.GovernmentAgencyId,
            Headquarters = dto.Headquarters,
            AdditionalStatement = dto.AdditionalStatement,
            // Shared fields (Type 2 & 4)
            CommercialRegNumber = dto.CommercialRegNumber,
            CompanyName = dto.CompanyName,
            // Unregistered Company fields (Type 4)
            CountryId = dto.CountryId,
            City = dto.City,
            Description = dto.Description,
            // Waqf fields (Type 7)
            WaqfName = dto.WaqfName,
            CourtDeedNumber = dto.CourtDeedNumber,
            CourtDeedDate = dto.CourtDeedDate,
            DeedSource = dto.DeedSource,
            WaqfSupervisoryTypeId = dto.WaqfSupervisoryTypeId,
            WaqfAgencyName = dto.WaqfAgencyName,
            WaqfRegionId = dto.WaqfRegionId,
            WaqfCityId = dto.WaqfCityId,
            WaqfDistrict = dto.WaqfDistrict,
            WaqfStreet = dto.WaqfStreet,
            WaqfBuildingNumber = dto.WaqfBuildingNumber,
            WaqfUnitNumber = dto.WaqfUnitNumber,
            WaqfPostalCode = dto.WaqfPostalCode,
            WaqfAdditionalCode = dto.WaqfAdditionalCode,
            WaqfAddressDescription = dto.WaqfAddressDescription,
            // NGO fields (Type 6)
            LicenseNumber = dto.LicenseNumber,
            LicenseSourceId = dto.LicenseSourceId,
            NGOName = dto.NGOName,
            LicenseDate = dto.LicenseDate,
            NGORegionId = dto.NGORegionId,
            NGOCityId = dto.NGOCityId,
            NGODistrict = dto.NGODistrict,
            NGOStreet = dto.NGOStreet,
            NGOBuildingNumber = dto.NGOBuildingNumber,
            NGOUnitNumber = dto.NGOUnitNumber,
            NGOPostalCode = dto.NGOPostalCode,
            NGOAdditionalCode = dto.NGOAdditionalCode,
            // Default values
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _defendantRepository.AddAsync(defendant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Defendant created with ID {DefendantId}", defendant.Id);

        // Create junction table entry
        var caseRequestDefendant = new CaseRequestDefendant
        {
            CaseRegistrationRequestId = requestId,
            DefendantId = defendant.Id,
            CreatedDate = DateTime.UtcNow
        };

        await _caseRequestDefendantRepository.AddAsync(caseRequestDefendant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(defendant.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create defendant");
    }

    public async Task<DefendantVM?> UpdateAsync(int id, DefendantCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        _logger.LogInformation("Updating defendant {DefendantId}", id);

        var defendant = await _defendantRepository.GetByIdAsync(id, cancellationToken);
        if (defendant == null)
        {
            _logger.LogWarning("Defendant {DefendantId} not found for update", id);
            return null;
        }

        // Check for duplicate defendant (same identity number and type on same request)
        // Applies to types that have identity numbers: Individual (1) and Business Owner (5)
        if ((dto.DefendantTypeId == TypeIndividual || dto.DefendantTypeId == 5) && !string.IsNullOrEmpty(dto.IdentityNumber))
        {
            var requestId = await _defendantRepository.GetRequestIdAsync(id, cancellationToken);
            if (requestId.HasValue)
            {
                var isDuplicate = await _defendantRepository.ExistsDuplicateExcludingAsync(requestId.Value, dto.IdentityNumber, dto.DefendantTypeId, id, cancellationToken);
                if (isDuplicate)
                {
                    _logger.LogWarning("Duplicate defendant detected on update: DefendantId={DefendantId}, IdentityNumber={IdentityNumber}, TypeId={TypeId}",
                        id, dto.IdentityNumber, dto.DefendantTypeId);
                    throw new InvalidOperationException("يوجد مدعى عليه بنفس رقم الهوية ونوع المدعى عليه مسبقاً في هذا الطلب");
                }
            }
        }

        // Update fields based on type
        defendant.DefendantTypeId = dto.DefendantTypeId;
        // Individual fields (Type 1)
        defendant.IdentityTypeId = dto.IdentityTypeId;
        defendant.IdentityNumber = dto.IdentityNumber;
        defendant.FirstName = dto.FirstName;
        defendant.FatherName = dto.FatherName;
        defendant.GrandfatherName = dto.GrandfatherName;
        defendant.TribeName = dto.TribeName;
        defendant.FamilyName = dto.FamilyName;
        defendant.BirthDate = dto.BirthDate;
        defendant.IdentityIssueDate = dto.IdentityIssueDate;
        defendant.IdentityExpiryDate = dto.IdentityExpiryDate;
        defendant.GenderId = dto.GenderId;
        defendant.NationalityId = dto.NationalityId;
        defendant.MobileNumber = dto.MobileNumber;
        defendant.Email = dto.Email;
        defendant.IndRegionId = dto.IndRegionId;
        defendant.IndCityId = dto.IndCityId;
        defendant.IndDistrict = dto.IndDistrict;
        defendant.IndStreet = dto.IndStreet;
        defendant.IndBuildingNumber = dto.IndBuildingNumber;
        defendant.IndUnitNumber = dto.IndUnitNumber;
        defendant.IndPostalCode = dto.IndPostalCode;
        defendant.IndAdditionalCode = dto.IndAdditionalCode;
        defendant.EmploymentStatusId = dto.EmploymentStatusId;
        defendant.Employer = dto.Employer;
        defendant.Occupation = dto.Occupation;
        // Work address fields (Type 1 - Private only)
        defendant.WorkRegionId = dto.WorkRegionId;
        defendant.WorkCityId = dto.WorkCityId;
        defendant.WorkDistrict = dto.WorkDistrict;
        defendant.WorkStreet = dto.WorkStreet;
        defendant.WorkBuildingNumber = dto.WorkBuildingNumber;
        defendant.WorkUnitNumber = dto.WorkUnitNumber;
        defendant.WorkPostalCode = dto.WorkPostalCode;
        defendant.WorkAdditionalCode = dto.WorkAdditionalCode;
        // Registered Company fields (Type 2)
        defendant.RegistrationStartDate = dto.RegistrationStartDate;
        defendant.RegistrationEndDate = dto.RegistrationEndDate;
        defendant.RegCompanyRegionId = dto.RegCompanyRegionId;
        defendant.RegCompanyCityId = dto.RegCompanyCityId;
        defendant.RegCompanyDistrict = dto.RegCompanyDistrict;
        defendant.RegCompanyStreet = dto.RegCompanyStreet;
        defendant.RegCompanyBuildingNumber = dto.RegCompanyBuildingNumber;
        defendant.RegCompanyUnitNumber = dto.RegCompanyUnitNumber;
        defendant.RegCompanyPostalCode = dto.RegCompanyPostalCode;
        defendant.RegCompanyAdditionalCode = dto.RegCompanyAdditionalCode;
        // Government Agency fields (Type 3)
        defendant.GovernmentAgencyId = dto.GovernmentAgencyId;
        defendant.Headquarters = dto.Headquarters;
        defendant.AdditionalStatement = dto.AdditionalStatement;
        // Shared fields (Type 2 & 4)
        defendant.CommercialRegNumber = dto.CommercialRegNumber;
        defendant.CompanyName = dto.CompanyName;
        // Unregistered Company fields (Type 4)
        defendant.CountryId = dto.CountryId;
        defendant.City = dto.City;
        defendant.Description = dto.Description;
        // Waqf fields (Type 7)
        defendant.WaqfName = dto.WaqfName;
        defendant.CourtDeedNumber = dto.CourtDeedNumber;
        defendant.CourtDeedDate = dto.CourtDeedDate;
        defendant.DeedSource = dto.DeedSource;
        defendant.WaqfSupervisoryTypeId = dto.WaqfSupervisoryTypeId;
        defendant.WaqfAgencyName = dto.WaqfAgencyName;
        defendant.WaqfRegionId = dto.WaqfRegionId;
        defendant.WaqfCityId = dto.WaqfCityId;
        defendant.WaqfDistrict = dto.WaqfDistrict;
        defendant.WaqfStreet = dto.WaqfStreet;
        defendant.WaqfBuildingNumber = dto.WaqfBuildingNumber;
        defendant.WaqfUnitNumber = dto.WaqfUnitNumber;
        defendant.WaqfPostalCode = dto.WaqfPostalCode;
        defendant.WaqfAdditionalCode = dto.WaqfAdditionalCode;
        defendant.WaqfAddressDescription = dto.WaqfAddressDescription;
        // NGO fields (Type 6)
        defendant.LicenseNumber = dto.LicenseNumber;
        defendant.LicenseSourceId = dto.LicenseSourceId;
        defendant.NGOName = dto.NGOName;
        defendant.LicenseDate = dto.LicenseDate;
        defendant.NGORegionId = dto.NGORegionId;
        defendant.NGOCityId = dto.NGOCityId;
        defendant.NGODistrict = dto.NGODistrict;
        defendant.NGOStreet = dto.NGOStreet;
        defendant.NGOBuildingNumber = dto.NGOBuildingNumber;
        defendant.NGOUnitNumber = dto.NGOUnitNumber;
        defendant.NGOPostalCode = dto.NGOPostalCode;
        defendant.NGOAdditionalCode = dto.NGOAdditionalCode;
        // Update display name
        defendant.FullName = dto.DefendantTypeId switch
        {
            TypeIndividual => BuildIndividualFullName(dto.FirstName, dto.FatherName, dto.GrandfatherName, dto.TribeName, dto.FamilyName),
            TypeRegisteredCompany => dto.CompanyName ?? "شركة مسجلة",
            TypeGovernmentAgency => dto.Headquarters ?? "جهة حكومية",
            TypeUnregisteredCompany => dto.CompanyName ?? "شركة غير مسجلة",
            TypeNGO => dto.NGOName ?? "جمعية/مؤسسة أهلية",
            TypeWaqf => dto.WaqfName ?? "وقف",
            _ => defendant.FullName
        };
        defendant.ModifiedDate = DateTime.UtcNow;

        await _defendantRepository.UpdateAsync(defendant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Defendant {DefendantId} updated successfully", id);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting defendant {DefendantId}", id);

        var defendant = await _defendantRepository.GetByIdAsync(id, cancellationToken);
        if (defendant == null)
        {
            _logger.LogWarning("Defendant {DefendantId} not found for deletion", id);
            return false;
        }

        // Soft delete the defendant
        defendant.IsDeleted = true;
        defendant.IsActive = false;
        defendant.ModifiedDate = DateTime.UtcNow;

        await _defendantRepository.UpdateAsync(defendant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Defendant {DefendantId} deleted successfully", id);
        return true;
    }

    #region Mapping Helpers

    private static string BuildIndividualFullName(string? firstName, string? fatherName, string? grandfatherName, string? tribeName, string? familyName)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(firstName)) parts.Add(firstName);
        if (!string.IsNullOrWhiteSpace(fatherName)) parts.Add(fatherName);
        if (!string.IsNullOrWhiteSpace(grandfatherName)) parts.Add(grandfatherName);
        if (!string.IsNullOrWhiteSpace(tribeName)) parts.Add(tribeName);
        if (!string.IsNullOrWhiteSpace(familyName)) parts.Add(familyName);
        return parts.Count > 0 ? string.Join(" ", parts) : "فرد";
    }

    private static string? GetNationalityArabicName(int? nationalityId)
    {
        if (nationalityId == null) return null;

        return (Nationality)nationalityId switch
        {
            Nationality.Saudi => "سعودي",
            Nationality.Emirati => "إماراتي",
            Nationality.Kuwaiti => "كويتي",
            Nationality.Bahraini => "بحريني",
            Nationality.Qatari => "قطري",
            Nationality.Omani => "عماني",
            Nationality.Egyptian => "مصري",
            Nationality.Jordanian => "أردني",
            Nationality.Lebanese => "لبناني",
            Nationality.Syrian => "سوري",
            Nationality.Iraqi => "عراقي",
            Nationality.Yemeni => "يمني",
            Nationality.Palestinian => "فلسطيني",
            Nationality.Sudanese => "سوداني",
            Nationality.Tunisian => "تونسي",
            Nationality.Moroccan => "مغربي",
            Nationality.Algerian => "جزائري",
            Nationality.Libyan => "ليبي",
            Nationality.Indian => "هندي",
            Nationality.Pakistani => "باكستاني",
            Nationality.Bangladeshi => "بنغلاديشي",
            Nationality.Filipino => "فلبيني",
            Nationality.Indonesian => "إندونيسي",
            Nationality.American => "أمريكي",
            Nationality.British => "بريطاني",
            Nationality.French => "فرنسي",
            Nationality.German => "ألماني",
            Nationality.Other => "أخرى",
            _ => null
        };
    }

    private static DefendantListVM MapToListVM(Defendant defendant)
    {
        var displayName = defendant.DefendantTypeId switch
        {
            TypeIndividual => defendant.FullName,
            TypeRegisteredCompany => defendant.CompanyName ?? defendant.FullName,
            TypeGovernmentAgency => defendant.GovernmentAgency?.NameAr ?? defendant.Headquarters ?? defendant.FullName,
            TypeUnregisteredCompany => defendant.CompanyName ?? defendant.FullName,
            TypeNGO => defendant.NGOName ?? defendant.FullName,
            TypeWaqf => defendant.WaqfName ?? defendant.FullName,
            _ => defendant.FullName
        };

        return new DefendantListVM
        {
            Id = defendant.Id,
            DefendantTypeId = defendant.DefendantTypeId,
            DefendantTypeNameAr = defendant.DefendantType?.NameAr ?? "",
            DisplayName = displayName
        };
    }

    private static DefendantVM MapToVM(Defendant defendant)
    {
        var displayName = defendant.DefendantTypeId switch
        {
            TypeIndividual => defendant.FullName,
            TypeRegisteredCompany => defendant.CompanyName ?? defendant.FullName,
            TypeGovernmentAgency => defendant.GovernmentAgency?.NameAr ?? defendant.Headquarters ?? defendant.FullName,
            TypeUnregisteredCompany => defendant.CompanyName ?? defendant.FullName,
            TypeNGO => defendant.NGOName ?? defendant.FullName,
            TypeWaqf => defendant.WaqfName ?? defendant.FullName,
            _ => defendant.FullName
        };

        // Get waqf supervisory type name
        string? waqfSupervisoryTypeName = defendant.WaqfSupervisoryTypeId switch
        {
            1 => "أهلية",
            2 => "حكومية",
            _ => null
        };

        // Get gender name
        string? genderName = defendant.GenderId switch
        {
            1 => "ذكر",
            2 => "أنثى",
            _ => null
        };

        // Get employment status name
        string? employmentStatusName = defendant.EmploymentStatusId switch
        {
            1 => "حكومي",
            2 => "خاص",
            3 => "بدون عمل",
            _ => null
        };

        return new DefendantVM
        {
            Id = defendant.Id,
            DefendantTypeId = defendant.DefendantTypeId,
            DefendantTypeNameAr = defendant.DefendantType?.NameAr ?? "",
            DisplayName = displayName,
            // Individual fields (Type 1)
            IdentityTypeId = defendant.IdentityTypeId,
            IdentityTypeName = defendant.IdentityType?.NameAr,
            IdentityNumber = defendant.IdentityNumber,
            FirstName = defendant.FirstName,
            FatherName = defendant.FatherName,
            GrandfatherName = defendant.GrandfatherName,
            TribeName = defendant.TribeName,
            FamilyName = defendant.FamilyName,
            BirthDate = defendant.BirthDate,
            IdentityIssueDate = defendant.IdentityIssueDate,
            IdentityExpiryDate = defendant.IdentityExpiryDate,
            GenderId = defendant.GenderId,
            GenderName = genderName,
            NationalityId = defendant.NationalityId,
            NationalityName = GetNationalityArabicName(defendant.NationalityId),
            MobileNumber = defendant.MobileNumber,
            Email = defendant.Email,
            IndRegionId = defendant.IndRegionId,
            IndRegionName = defendant.IndRegion?.NameAr,
            IndCityId = defendant.IndCityId,
            IndCityName = defendant.IndCity?.NameAr,
            IndDistrict = defendant.IndDistrict,
            IndStreet = defendant.IndStreet,
            IndBuildingNumber = defendant.IndBuildingNumber,
            IndUnitNumber = defendant.IndUnitNumber,
            IndPostalCode = defendant.IndPostalCode,
            IndAdditionalCode = defendant.IndAdditionalCode,
            EmploymentStatusId = defendant.EmploymentStatusId,
            EmploymentStatusName = employmentStatusName,
            Employer = defendant.Employer,
            Occupation = defendant.Occupation,
            // Work address fields (Type 1 - Private only)
            WorkRegionId = defendant.WorkRegionId,
            WorkRegionName = defendant.WorkRegion?.NameAr,
            WorkCityId = defendant.WorkCityId,
            WorkCityName = defendant.WorkCity?.NameAr,
            WorkDistrict = defendant.WorkDistrict,
            WorkStreet = defendant.WorkStreet,
            WorkBuildingNumber = defendant.WorkBuildingNumber,
            WorkUnitNumber = defendant.WorkUnitNumber,
            WorkPostalCode = defendant.WorkPostalCode,
            WorkAdditionalCode = defendant.WorkAdditionalCode,
            // Registered Company fields (Type 2)
            RegistrationStartDate = defendant.RegistrationStartDate,
            RegistrationEndDate = defendant.RegistrationEndDate,
            RegCompanyRegionId = defendant.RegCompanyRegionId,
            RegCompanyRegionName = defendant.RegCompanyRegion?.NameAr,
            RegCompanyCityId = defendant.RegCompanyCityId,
            RegCompanyCityName = defendant.RegCompanyCity?.NameAr,
            RegCompanyDistrict = defendant.RegCompanyDistrict,
            RegCompanyStreet = defendant.RegCompanyStreet,
            RegCompanyBuildingNumber = defendant.RegCompanyBuildingNumber,
            RegCompanyUnitNumber = defendant.RegCompanyUnitNumber,
            RegCompanyPostalCode = defendant.RegCompanyPostalCode,
            RegCompanyAdditionalCode = defendant.RegCompanyAdditionalCode,
            // Government Agency fields (Type 3)
            GovernmentAgencyId = defendant.GovernmentAgencyId,
            GovernmentAgencyName = defendant.GovernmentAgency?.NameAr,
            Headquarters = defendant.Headquarters,
            AdditionalStatement = defendant.AdditionalStatement,
            // Shared fields (Type 2 & 4)
            CommercialRegNumber = defendant.CommercialRegNumber,
            CompanyName = defendant.CompanyName,
            // Unregistered Company fields (Type 4)
            CountryId = defendant.CountryId,
            CountryName = defendant.Country?.NameAr,
            City = defendant.City,
            Description = defendant.Description,
            // Waqf fields (Type 7)
            WaqfName = defendant.WaqfName,
            CourtDeedNumber = defendant.CourtDeedNumber,
            CourtDeedDate = defendant.CourtDeedDate,
            DeedSource = defendant.DeedSource,
            WaqfSupervisoryTypeId = defendant.WaqfSupervisoryTypeId,
            WaqfSupervisoryTypeName = waqfSupervisoryTypeName,
            WaqfAgencyName = defendant.WaqfAgencyName,
            WaqfRegionId = defendant.WaqfRegionId,
            WaqfRegionName = defendant.WaqfRegion?.NameAr,
            WaqfCityId = defendant.WaqfCityId,
            WaqfCityName = defendant.WaqfCity?.NameAr,
            WaqfDistrict = defendant.WaqfDistrict,
            WaqfStreet = defendant.WaqfStreet,
            WaqfBuildingNumber = defendant.WaqfBuildingNumber,
            WaqfUnitNumber = defendant.WaqfUnitNumber,
            WaqfPostalCode = defendant.WaqfPostalCode,
            WaqfAdditionalCode = defendant.WaqfAdditionalCode,
            WaqfAddressDescription = defendant.WaqfAddressDescription,
            // NGO fields (Type 6)
            LicenseNumber = defendant.LicenseNumber,
            LicenseSourceId = defendant.LicenseSourceId,
            LicenseSourceName = defendant.LicenseSource?.NameAr,
            NGOName = defendant.NGOName,
            LicenseDate = defendant.LicenseDate,
            NGORegionId = defendant.NGORegionId,
            NGORegionName = defendant.NGORegion?.NameAr,
            NGOCityId = defendant.NGOCityId,
            NGOCityName = defendant.NGOCity?.NameAr,
            NGODistrict = defendant.NGODistrict,
            NGOStreet = defendant.NGOStreet,
            NGOBuildingNumber = defendant.NGOBuildingNumber,
            NGOUnitNumber = defendant.NGOUnitNumber,
            NGOPostalCode = defendant.NGOPostalCode,
            NGOAdditionalCode = defendant.NGOAdditionalCode,
            CreatedDate = defendant.CreatedDate
        };
    }

    #endregion
}
