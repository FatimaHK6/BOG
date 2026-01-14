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
    /// Foreign key to residence address.
    /// </summary>
    public int? ResidenceAddressId { get; set; }

    /// <summary>
    /// Foreign key to work address.
    /// </summary>
    public int? WorkAddressId { get; set; }

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

    #region Government Agency Data

    /// <summary>
    /// Foreign key to GovernmentAgency.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Additional statement (بيان إضافي) - max 4000 characters.
    /// </summary>
    public string? AdditionalStatement { get; set; }

    #endregion

    #region Society/NGO Data

    /// <summary>
    /// License number.
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// License source/issuer.
    /// </summary>
    public string? LicenseSource { get; set; }

    /// <summary>
    /// License date.
    /// </summary>
    public DateTime? LicenseDate { get; set; }

    #endregion

    #region Waqf Data

    /// <summary>
    /// Court deed number.
    /// </summary>
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Deed date.
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source/issuer.
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf oversight type (خاصة/حكومية).
    /// </summary>
    public string? WaqfOversightType { get; set; }

    #endregion

    #region Employment Data (for Individual)

    /// <summary>
    /// Employer name.
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
    /// Navigation property for residence address.
    /// </summary>
    public virtual Address? ResidenceAddress { get; set; }

    /// <summary>
    /// Navigation property for work address.
    /// </summary>
    public virtual Address? WorkAddress { get; set; }

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
