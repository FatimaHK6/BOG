using BOG.DbModel.Entities.Common;
using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for plaintiff (مدعي) - many-to-many with CaseRegistrationRequest.
/// </summary>
public class Plaintiff : BaseEntity
{
    /// <summary>
    /// Foreign key to PlaintiffType.
    /// </summary>
    public int PlaintiffTypeId { get; set; }

    #region Personal Data (for Individual types)

    /// <summary>
    /// Foreign key to IdentityType.
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number (رقم الهوية) - 10 or 20 characters.
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// First name (الاسم الأول).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Father's name (اسم الأب).
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather's name (اسم الجد).
    /// </summary>
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Family name (اسم العائلة).
    /// </summary>
    public string? FamilyName { get; set; }

    /// <summary>
    /// Clan name (اسم القبيلة).
    /// </summary>
    public string? ClanName { get; set; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gender (ذكر/أنثى).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality ID.
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Identity issue date.
    /// </summary>
    public DateTime? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date.
    /// </summary>
    public DateTime? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Document number for individual without ID (رقم الوثيقة) - SRS 6.3.10.
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Foreign key to DataSource (مصدر البيانات - أبشر/المستخدم).
    /// </summary>
    public int? DataSourceId { get; set; }

    #endregion

    #region Contact Info

    /// <summary>
    /// Mobile number (10 digits, starts with 05).
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    #endregion

    #region Address References

    /// <summary>
    /// Foreign key to residence address (for Individual types).
    /// </summary>
    public int? ResidenceAddressId { get; set; }

    /// <summary>
    /// Foreign key to work address (for Individual types).
    /// </summary>
    public int? WorkAddressId { get; set; }

    /// <summary>
    /// Foreign key to business/institution address (for Type 3 - Business Owner).
    /// </summary>
    public int? BusinessAddressId { get; set; }

    /// <summary>
    /// Foreign key to company address (for Type 4 - Registered Company).
    /// </summary>
    public int? CompanyAddressId { get; set; }

    /// <summary>
    /// Foreign key to NGO address (for Type 7 - NGO).
    /// </summary>
    public int? NGOAddressId { get; set; }

    /// <summary>
    /// Foreign key to Waqf address (for Type 8 - Waqf).
    /// </summary>
    public int? WaqfAddressId { get; set; }

    /// <summary>
    /// Foreign key to selected address for correspondence (العنوان المختار).
    /// </summary>
    public int? SelectedAddressId { get; set; }

    #endregion

    #region Business/Company Data

    /// <summary>
    /// Commercial registration number (10 digits).
    /// </summary>
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Commercial registration start date.
    /// </summary>
    public DateTime? CRStartDate { get; set; }

    /// <summary>
    /// Commercial registration end date.
    /// </summary>
    public DateTime? CREndDate { get; set; }

    #endregion

    #region Unregistered Company Data (Type 5)

    /// <summary>
    /// Company address text for unregistered company (عنوان الشركة).
    /// </summary>
    public string? UnregisteredCompanyAddress { get; set; }

    /// <summary>
    /// Country ID for unregistered company (الدولة).
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// City name for unregistered company (المدينة).
    /// </summary>
    public string? UnregisteredCompanyCity { get; set; }

    /// <summary>
    /// Approximate description (وصف تقريبي) - max 1000 characters.
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Government Agency Data (Type 6)

    /// <summary>
    /// Foreign key to GovernmentAgency.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Headquarters (المقر) - auto-filled based on agency.
    /// </summary>
    public string? Headquarters { get; set; }

    /// <summary>
    /// Additional statement (بيان إضافي) - max 4000 characters.
    /// </summary>
    public string? AdditionalStatement { get; set; }

    #endregion

    #region Society/NGO Data (Type 7)

    /// <summary>
    /// License number (رقم الترخيص) - 10 digits.
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Foreign key to LicenseSource (مصدر الترخيص).
    /// </summary>
    public int? LicenseSourceId { get; set; }

    /// <summary>
    /// NGO name (اسم الجمعية/المؤسسة).
    /// </summary>
    public string? NGOName { get; set; }

    /// <summary>
    /// License date (تاريخ الترخيص).
    /// </summary>
    public DateTime? LicenseDate { get; set; }

    #endregion

    #region Waqf Data (Type 8)

    /// <summary>
    /// Court deed number (رقم صك المحكمة) - 10 digits.
    /// </summary>
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Waqf name (اسم الوقف).
    /// </summary>
    public string? WaqfName { get; set; }

    /// <summary>
    /// Deed date (تاريخ صك المحكمة).
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source/issuer (مصدر الصك).
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf oversight type (نظارة الوقف - خاصة/حكومية).
    /// </summary>
    public string? WaqfOversightType { get; set; }

    /// <summary>
    /// Waqf agency name (اسم الجهة) - required if WaqfOversightType is حكومية.
    /// </summary>
    public string? WaqfAgencyName { get; set; }

    /// <summary>
    /// Waqf description (وصف تقريبي).
    /// </summary>
    public string? WaqfDescription { get; set; }

    #endregion

    #region Employment Data (for Individual)

    /// <summary>
    /// Employment status ID: 1-Government, 2-Private, 3-Unemployed.
    /// Required for types 1 (Individual) and 3 (Business Owner).
    /// </summary>
    public int? EmploymentStatusId { get; set; }

    /// <summary>
    /// Employer name - required when EmploymentStatusId is 1 (Government) or 2 (Private).
    /// BC01: Hidden when EmploymentStatusId is 3 (Unemployed).
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Profession/occupation.
    /// </summary>
    public string? Profession { get; set; }

    #endregion

    #region Status Flags

    /// <summary>
    /// Whether this plaintiff is the applicant (مقدم الطلب).
    /// </summary>
    public bool IsApplicant { get; set; }

    /// <summary>
    /// Whether the plaintiff is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Navigation property for plaintiff type.
    /// </summary>
    public virtual PlaintiffType PlaintiffType { get; set; } = null!;

    /// <summary>
    /// Navigation property for identity type.
    /// </summary>
    public virtual IdentityType? IdentityType { get; set; }

    /// <summary>
    /// Navigation property for data source.
    /// </summary>
    public virtual DataSource? DataSource { get; set; }

    /// <summary>
    /// Navigation property for government agency.
    /// </summary>
    public virtual GovernmentAgency? GovernmentAgency { get; set; }

    /// <summary>
    /// Navigation property for license source.
    /// </summary>
    public virtual LicenseSource? LicenseSource { get; set; }

    /// <summary>
    /// Navigation property for country (for unregistered company).
    /// </summary>
    public virtual Country? Country { get; set; }

    /// <summary>
    /// Navigation property for residence address.
    /// </summary>
    public virtual Address? ResidenceAddress { get; set; }

    /// <summary>
    /// Navigation property for work address.
    /// </summary>
    public virtual Address? WorkAddress { get; set; }

    /// <summary>
    /// Navigation property for business address.
    /// </summary>
    public virtual Address? BusinessAddress { get; set; }

    /// <summary>
    /// Navigation property for company address.
    /// </summary>
    public virtual Address? CompanyAddress { get; set; }

    /// <summary>
    /// Navigation property for NGO address.
    /// </summary>
    public virtual Address? NGOAddress { get; set; }

    /// <summary>
    /// Navigation property for Waqf address.
    /// </summary>
    public virtual Address? WaqfAddress { get; set; }

    /// <summary>
    /// Navigation property for selected address.
    /// </summary>
    public virtual Address? SelectedAddress { get; set; }

    /// <summary>
    /// Collection of case request associations (many-to-many).
    /// </summary>
    public virtual ICollection<CaseRequestPlaintiff> CaseRequestPlaintiffs { get; set; } = new List<CaseRequestPlaintiff>();

    /// <summary>
    /// Collection of representatives.
    /// </summary>
    public virtual ICollection<Representative> Representatives { get; set; } = new List<Representative>();

    /// <summary>
    /// Collection of attachments.
    /// </summary>
    public virtual ICollection<PlaintiffAttachment> Attachments { get; set; } = new List<PlaintiffAttachment>();

    #endregion
}
