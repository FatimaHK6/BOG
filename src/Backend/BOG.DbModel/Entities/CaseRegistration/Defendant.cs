using BOG.DbModel.Entities.Common;
using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for defendant (مدعى عليه) - many-to-many with CaseRegistrationRequest.
/// </summary>
public class Defendant : BaseEntity
{
    /// <summary>
    /// Foreign key to DefendantType.
    /// </summary>
    public int DefendantTypeId { get; set; }

    /// <summary>
    /// Full name (required) - max 200 characters.
    /// </summary>
    public string FullName { get; set; } = null!;

    #region Individual Data (Type 1)

    /// <summary>
    /// Foreign key to IdentityType.
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number (10 digits).
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Foreign key to DataSource (مصدر البيانات - أبشر/المستخدم).
    /// </summary>
    public int? DataSourceId { get; set; }

    /// <summary>
    /// First name (الاسم الأول) - required for Individual.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Father name (اسم الأب) - conditional (required if IdentityType = National ID).
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather name (اسم الجد) - optional.
    /// </summary>
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Tribe name (اسم الفخذ) - optional.
    /// </summary>
    public string? TribeName { get; set; }

    /// <summary>
    /// Family name (اسم العائلة) - required for Individual.
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// Birth date (تاريخ الميلاد) - optional, cannot be in the future.
    /// </summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Identity issue date (تاريخ إصدار الهوية) - required for Business Owner.
    /// </summary>
    public DateOnly? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date (تاريخ انتهاء الهوية) - required for Business Owner.
    /// </summary>
    public DateOnly? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Gender ID (الجنس) - required for Individual (1=Male, 2=Female).
    /// </summary>
    public int? GenderId { get; set; }

    /// <summary>
    /// Nationality ID (الجنسية) - required for Individual.
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Mobile number (رقم الجوال) - 10 digits starting with 05.
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address (البريد الإلكتروني) - optional.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Individual residence address - Region ID.
    /// </summary>
    public int? IndRegionId { get; set; }

    /// <summary>
    /// Individual residence address - City ID.
    /// </summary>
    public int? IndCityId { get; set; }

    /// <summary>
    /// Individual residence address - District (الحي).
    /// </summary>
    public string? IndDistrict { get; set; }

    /// <summary>
    /// Individual residence address - Street (الشارع).
    /// </summary>
    public string? IndStreet { get; set; }

    /// <summary>
    /// Individual residence address - Building number (4 digits).
    /// </summary>
    public string? IndBuildingNumber { get; set; }

    /// <summary>
    /// Individual residence address - Unit number (4 digits).
    /// </summary>
    public string? IndUnitNumber { get; set; }

    /// <summary>
    /// Individual residence address - Postal code (5 digits).
    /// </summary>
    public string? IndPostalCode { get; set; }

    /// <summary>
    /// Individual residence address - Additional code (4 digits).
    /// </summary>
    public string? IndAdditionalCode { get; set; }

    /// <summary>
    /// Employment status (حالة العمل) - 1=Government, 2=Private, 3=Unemployed.
    /// </summary>
    public int? EmploymentStatusId { get; set; }

    /// <summary>
    /// Employer name (جهة العمل) - conditional (shown if employed).
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Occupation (المهنة) - conditional (shown if employed).
    /// </summary>
    public string? Occupation { get; set; }

    /// <summary>
    /// Work address - Region ID (المنطقة) - shown only if EmploymentStatus = Private (2).
    /// </summary>
    public int? WorkRegionId { get; set; }

    /// <summary>
    /// Work address - City ID (المدينة) - shown only if EmploymentStatus = Private (2).
    /// </summary>
    public int? WorkCityId { get; set; }

    /// <summary>
    /// Work address - District (الحي).
    /// </summary>
    public string? WorkDistrict { get; set; }

    /// <summary>
    /// Work address - Street (الشارع).
    /// </summary>
    public string? WorkStreet { get; set; }

    /// <summary>
    /// Work address - Building number (4 digits).
    /// </summary>
    public string? WorkBuildingNumber { get; set; }

    /// <summary>
    /// Work address - Unit number (4 digits).
    /// </summary>
    public string? WorkUnitNumber { get; set; }

    /// <summary>
    /// Work address - Postal code (5 digits).
    /// </summary>
    public string? WorkPostalCode { get; set; }

    /// <summary>
    /// Work address - Additional code (4 digits).
    /// </summary>
    public string? WorkAdditionalCode { get; set; }

    #endregion

    #region Address Data

    /// <summary>
    /// Address text (if not structured).
    /// </summary>
    public string? AddressText { get; set; }

    /// <summary>
    /// Foreign key to Address (if structured).
    /// </summary>
    public int? AddressId { get; set; }

    #endregion

    #region Registered Company Data (Type 2)

    /// <summary>
    /// Registration start date (Type 2).
    /// </summary>
    public DateOnly? RegistrationStartDate { get; set; }

    /// <summary>
    /// Registration end date (Type 2).
    /// </summary>
    public DateOnly? RegistrationEndDate { get; set; }

    /// <summary>
    /// Registered company address - Region ID.
    /// </summary>
    public int? RegCompanyRegionId { get; set; }

    /// <summary>
    /// Registered company address - City ID.
    /// </summary>
    public int? RegCompanyCityId { get; set; }

    /// <summary>
    /// Registered company address - District.
    /// </summary>
    public string? RegCompanyDistrict { get; set; }

    /// <summary>
    /// Registered company address - Street.
    /// </summary>
    public string? RegCompanyStreet { get; set; }

    /// <summary>
    /// Registered company address - Building number (4 digits).
    /// </summary>
    public string? RegCompanyBuildingNumber { get; set; }

    /// <summary>
    /// Registered company address - Unit number.
    /// </summary>
    public string? RegCompanyUnitNumber { get; set; }

    /// <summary>
    /// Registered company address - Postal code (5 digits).
    /// </summary>
    public string? RegCompanyPostalCode { get; set; }

    /// <summary>
    /// Registered company address - Additional code (4 digits).
    /// </summary>
    public string? RegCompanyAdditionalCode { get; set; }

    #endregion

    #region Unregistered Company Data (Type 4)

    /// <summary>
    /// Commercial registration number (max 20 chars).
    /// </summary>
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Company name (max 200 chars).
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Foreign key to Country.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// City name (max 100 chars).
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Description (max 1000 chars).
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Government Agency Data

    /// <summary>
    /// Foreign key to GovernmentAgency.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Headquarters location.
    /// </summary>
    public string? Headquarters { get; set; }

    /// <summary>
    /// Additional statement (بيان إضافي) - max 4000 characters.
    /// </summary>
    public string? AdditionalStatement { get; set; }

    #endregion

    #region Waqf Data (Type 7)

    /// <summary>
    /// Waqf name (max 200 chars).
    /// </summary>
    public string? WaqfName { get; set; }

    /// <summary>
    /// Court deed number (10 chars).
    /// </summary>
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Court deed date.
    /// </summary>
    public DateOnly? CourtDeedDate { get; set; }

    /// <summary>
    /// Deed source (max 100 chars).
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf supervisory type (1=أهلية, 2=حكومية).
    /// </summary>
    public int? WaqfSupervisoryTypeId { get; set; }

    /// <summary>
    /// Agency name - only if WaqfSupervisoryTypeId = 2 (حكومية).
    /// </summary>
    public string? WaqfAgencyName { get; set; }

    /// <summary>
    /// Waqf address - Region ID.
    /// </summary>
    public int? WaqfRegionId { get; set; }

    /// <summary>
    /// Waqf address - City ID.
    /// </summary>
    public int? WaqfCityId { get; set; }

    /// <summary>
    /// Waqf address - District.
    /// </summary>
    public string? WaqfDistrict { get; set; }

    /// <summary>
    /// Waqf address - Street (max 200 chars).
    /// </summary>
    public string? WaqfStreet { get; set; }

    /// <summary>
    /// Waqf address - Building number (4 digits).
    /// </summary>
    public string? WaqfBuildingNumber { get; set; }

    /// <summary>
    /// Waqf address - Unit number.
    /// </summary>
    public string? WaqfUnitNumber { get; set; }

    /// <summary>
    /// Waqf address - Postal code (5 digits).
    /// </summary>
    public string? WaqfPostalCode { get; set; }

    /// <summary>
    /// Waqf address - Additional code (4 digits).
    /// </summary>
    public string? WaqfAdditionalCode { get; set; }

    /// <summary>
    /// Waqf address - Description (required).
    /// </summary>
    public string? WaqfAddressDescription { get; set; }

    #endregion

    #region NGO/Society Data (Type 6)

    /// <summary>
    /// License number (رقم الترخيص) - 10 digits.
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Foreign key to LicenseSource (مصدر الترخيص).
    /// </summary>
    public int? LicenseSourceId { get; set; }

    /// <summary>
    /// NGO name (اسم الجمعية/المؤسسة) - max 200 chars.
    /// </summary>
    public string? NGOName { get; set; }

    /// <summary>
    /// License date (تاريخ الترخيص).
    /// </summary>
    public DateOnly? LicenseDate { get; set; }

    /// <summary>
    /// NGO address - Region ID.
    /// </summary>
    public int? NGORegionId { get; set; }

    /// <summary>
    /// NGO address - City ID.
    /// </summary>
    public int? NGOCityId { get; set; }

    /// <summary>
    /// NGO address - District (max 100 chars).
    /// </summary>
    public string? NGODistrict { get; set; }

    /// <summary>
    /// NGO address - Street (max 200 chars).
    /// </summary>
    public string? NGOStreet { get; set; }

    /// <summary>
    /// NGO address - Building number (4 digits).
    /// </summary>
    public string? NGOBuildingNumber { get; set; }

    /// <summary>
    /// NGO address - Unit number.
    /// </summary>
    public string? NGOUnitNumber { get; set; }

    /// <summary>
    /// NGO address - Postal code (5 digits).
    /// </summary>
    public string? NGOPostalCode { get; set; }

    /// <summary>
    /// NGO address - Additional code (4 digits).
    /// </summary>
    public string? NGOAdditionalCode { get; set; }

    #endregion

    /// <summary>
    /// Whether the defendant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Navigation property for defendant type.
    /// </summary>
    public virtual DefendantType DefendantType { get; set; } = null!;

    /// <summary>
    /// Navigation property for identity type.
    /// </summary>
    public virtual IdentityType? IdentityType { get; set; }

    /// <summary>
    /// Navigation property for data source.
    /// </summary>
    public virtual DataSource? DataSource { get; set; }

    /// <summary>
    /// Navigation property for address.
    /// </summary>
    public virtual Address? Address { get; set; }

    /// <summary>
    /// Navigation property for government agency.
    /// </summary>
    public virtual GovernmentAgency? GovernmentAgency { get; set; }

    /// <summary>
    /// Navigation property for country.
    /// </summary>
    public virtual Country? Country { get; set; }

    /// <summary>
    /// Navigation property for waqf region.
    /// </summary>
    public virtual Region? WaqfRegion { get; set; }

    /// <summary>
    /// Navigation property for waqf city.
    /// </summary>
    public virtual City? WaqfCity { get; set; }

    /// <summary>
    /// Navigation property for registered company region.
    /// </summary>
    public virtual Region? RegCompanyRegion { get; set; }

    /// <summary>
    /// Navigation property for registered company city.
    /// </summary>
    public virtual City? RegCompanyCity { get; set; }

    /// <summary>
    /// Navigation property for license source.
    /// </summary>
    public virtual LicenseSource? LicenseSource { get; set; }

    /// <summary>
    /// Navigation property for NGO region.
    /// </summary>
    public virtual Region? NGORegion { get; set; }

    /// <summary>
    /// Navigation property for NGO city.
    /// </summary>
    public virtual City? NGOCity { get; set; }

    /// <summary>
    /// Navigation property for individual residence region.
    /// </summary>
    public virtual Region? IndRegion { get; set; }

    /// <summary>
    /// Navigation property for individual residence city.
    /// </summary>
    public virtual City? IndCity { get; set; }

    /// <summary>
    /// Navigation property for work region.
    /// </summary>
    public virtual Region? WorkRegion { get; set; }

    /// <summary>
    /// Navigation property for work city.
    /// </summary>
    public virtual City? WorkCity { get; set; }

    /// <summary>
    /// Collection of case request associations (many-to-many).
    /// </summary>
    public virtual ICollection<CaseRequestDefendant> CaseRequestDefendants { get; set; } = new List<CaseRequestDefendant>();

    #endregion
}
