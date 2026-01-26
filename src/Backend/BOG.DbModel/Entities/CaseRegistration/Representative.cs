using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for representative (ممثل) of a plaintiff.
/// </summary>
public class Representative : BaseEntity
{
    /// <summary>
    /// Foreign key to Plaintiff.
    /// </summary>
    public int PlaintiffId { get; set; }

    /// <summary>
    /// Foreign key to RepresentativeType.
    /// </summary>
    public int RepresentativeTypeId { get; set; }

    #region Personal Data

    /// <summary>
    /// Foreign key to IdentityType.
    /// </summary>
    public int IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number.
    /// </summary>
    public string IdentityNumber { get; set; } = null!;

    /// <summary>
    /// First name (الاسم الأول).
    /// </summary>
    public string FirstName { get; set; } = null!;

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
    public string FamilyName { get; set; } = null!;

    /// <summary>
    /// Clan name (اسم القبيلة).
    /// </summary>
    public string? ClanName { get; set; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gender (الجنس): male/female.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Foreign key to Nationality (الجنسية).
    /// </summary>
    public int? NationalityId { get; set; }

    /// <summary>
    /// Identity issue date (تاريخ إصدار الهوية).
    /// </summary>
    public DateTime? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date (تاريخ انتهاء الهوية).
    /// </summary>
    public DateTime? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Foreign key to DataSource (مصدر البيانات - أبشر/المستخدم).
    /// </summary>
    public int? DataSourceId { get; set; }

    #endregion

    #region Residence Address (عنوان السكن - 6.3.2)

    /// <summary>
    /// Region ID for residence address (المنطقة).
    /// </summary>
    public int? ResidenceRegionId { get; set; }

    /// <summary>
    /// City ID for residence address (المدينة).
    /// </summary>
    public int? ResidenceCityId { get; set; }

    /// <summary>
    /// District for residence address (الحي).
    /// </summary>
    public string? ResidenceDistrict { get; set; }

    /// <summary>
    /// Street for residence address (الشارع).
    /// </summary>
    public string? ResidenceStreet { get; set; }

    /// <summary>
    /// Building number for residence address (رقم المبنى).
    /// </summary>
    public string? ResidenceBuildingNumber { get; set; }

    /// <summary>
    /// Unit number for residence address (رقم الوحدة).
    /// </summary>
    public string? ResidenceUnitNumber { get; set; }

    /// <summary>
    /// Postal code for residence address (الرمز البريدي).
    /// </summary>
    public string? ResidencePostalCode { get; set; }

    /// <summary>
    /// Additional code for residence address (الرمز الإضافي).
    /// </summary>
    public string? ResidenceAdditionalCode { get; set; }

    #endregion

    #region Employment Data (بيانات العمل)

    /// <summary>
    /// Employment status (حالة العمل): government/private/unemployed.
    /// </summary>
    public string? EmploymentStatus { get; set; }

    /// <summary>
    /// Employer name (جهة العمل) - conditional BC01.
    /// </summary>
    public string? Employer { get; set; }

    /// <summary>
    /// Profession (المهنة).
    /// </summary>
    public string? Profession { get; set; }

    #endregion

    #region Work Address (عنوان العمل - 6.3.2) - Conditional BC02

    /// <summary>
    /// Region ID for work address (المنطقة).
    /// </summary>
    public int? WorkRegionId { get; set; }

    /// <summary>
    /// City ID for work address (المدينة).
    /// </summary>
    public int? WorkCityId { get; set; }

    /// <summary>
    /// District for work address (الحي).
    /// </summary>
    public string? WorkDistrict { get; set; }

    /// <summary>
    /// Street for work address (الشارع).
    /// </summary>
    public string? WorkStreet { get; set; }

    /// <summary>
    /// Building number for work address (رقم المبنى).
    /// </summary>
    public string? WorkBuildingNumber { get; set; }

    /// <summary>
    /// Unit number for work address (رقم الوحدة).
    /// </summary>
    public string? WorkUnitNumber { get; set; }

    /// <summary>
    /// Postal code for work address (الرمز البريدي).
    /// </summary>
    public string? WorkPostalCode { get; set; }

    /// <summary>
    /// Additional code for work address (الرمز الإضافي).
    /// </summary>
    public string? WorkAdditionalCode { get; set; }

    #endregion

    #region Contact Info

    /// <summary>
    /// Mobile number.
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    #endregion

    #region Authorization Document

    /// <summary>
    /// Authorization/Power of Attorney number (رقم الوكالة/القرار/الصك).
    /// </summary>
    public string? AuthorizationNumber { get; set; }

    /// <summary>
    /// Authorization date.
    /// </summary>
    public DateTime? AuthorizationDate { get; set; }

    /// <summary>
    /// Authorization source/issuer.
    /// </summary>
    public string? AuthorizationSource { get; set; }

    /// <summary>
    /// Authorization source type (كتابة عدل/محكمة/وزارة خارجية).
    /// </summary>
    public string? AuthorizationSourceType { get; set; }

    #endregion

    #region Liquidator Data (مصفي - 6.3.12)

    /// <summary>
    /// Decision number (رقم القرار) for Liquidator.
    /// </summary>
    public string? DecisionNumber { get; set; }

    /// <summary>
    /// Decision date (تاريخ القرار) for Liquidator.
    /// </summary>
    public DateTime? DecisionDate { get; set; }

    /// <summary>
    /// Decision source (مصدر القرار) for Liquidator.
    /// </summary>
    public string? DecisionSource { get; set; }

    #endregion

    #region Guardian Data (ولي - 6.3.16)

    /// <summary>
    /// Deed number (رقم الصك) for Guardian.
    /// </summary>
    public string? DeedNumber { get; set; }

    /// <summary>
    /// Deed date (تاريخ الصك) for Guardian.
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source (مصدر الصك) for Guardian.
    /// </summary>
    public string? DeedSource { get; set; }

    /// <summary>
    /// Guardianship type for Guardian (ولي): طبيعية/مكتسبة.
    /// </summary>
    public string? GuardianshipType { get; set; }

    #endregion

    /// <summary>
    /// Whether the representative is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region Navigation Properties

    /// <summary>
    /// Navigation property for plaintiff.
    /// </summary>
    public virtual Plaintiff Plaintiff { get; set; } = null!;

    /// <summary>
    /// Navigation property for representative type.
    /// </summary>
    public virtual RepresentativeType RepresentativeType { get; set; } = null!;

    /// <summary>
    /// Navigation property for identity type.
    /// </summary>
    public virtual IdentityType IdentityType { get; set; } = null!;

    /// <summary>
    /// Navigation property for data source.
    /// </summary>
    public virtual DataSource? DataSource { get; set; }

    #endregion
}
