using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Representative;

/// <summary>
/// DTO for updating a representative.
/// Identity number and type cannot be changed.
/// </summary>
public class RepresentativeUpdateDTO
{
    #region Personal Data

    /// <summary>
    /// First name.
    /// </summary>
    [StringLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Father's name.
    /// </summary>
    [StringLength(100)]
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather's name.
    /// </summary>
    [StringLength(100)]
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Family name.
    /// </summary>
    [StringLength(100)]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Clan name (اسم الفخذ).
    /// </summary>
    [StringLength(100)]
    public string? ClanName { get; set; }

    /// <summary>
    /// Gender (الجنس): male/female.
    /// </summary>
    [StringLength(10)]
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality ID (الجنسية).
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
    [StringLength(100)]
    public string? ResidenceDistrict { get; set; }

    /// <summary>
    /// Street for residence address (الشارع).
    /// </summary>
    [StringLength(100)]
    public string? ResidenceStreet { get; set; }

    /// <summary>
    /// Building number for residence address (رقم المبنى).
    /// </summary>
    [StringLength(10)]
    public string? ResidenceBuildingNumber { get; set; }

    /// <summary>
    /// Unit number for residence address (رقم الوحدة).
    /// </summary>
    [StringLength(10)]
    public string? ResidenceUnitNumber { get; set; }

    /// <summary>
    /// Postal code for residence address (الرمز البريدي).
    /// </summary>
    [StringLength(5)]
    public string? ResidencePostalCode { get; set; }

    /// <summary>
    /// Additional code for residence address (الرمز الإضافي).
    /// </summary>
    [StringLength(4)]
    public string? ResidenceAdditionalCode { get; set; }

    #endregion

    #region Employment Data (بيانات العمل)

    /// <summary>
    /// Employment status (حالة العمل): government/private/unemployed.
    /// </summary>
    [StringLength(20)]
    public string? EmploymentStatus { get; set; }

    /// <summary>
    /// Employer name (جهة العمل).
    /// </summary>
    [StringLength(200)]
    public string? Employer { get; set; }

    /// <summary>
    /// Profession (المهنة).
    /// </summary>
    [StringLength(200)]
    public string? Profession { get; set; }

    #endregion

    #region Work Address (عنوان العمل - 6.3.2)

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
    [StringLength(100)]
    public string? WorkDistrict { get; set; }

    /// <summary>
    /// Street for work address (الشارع).
    /// </summary>
    [StringLength(100)]
    public string? WorkStreet { get; set; }

    /// <summary>
    /// Building number for work address (رقم المبنى).
    /// </summary>
    [StringLength(10)]
    public string? WorkBuildingNumber { get; set; }

    /// <summary>
    /// Unit number for work address (رقم الوحدة).
    /// </summary>
    [StringLength(10)]
    public string? WorkUnitNumber { get; set; }

    /// <summary>
    /// Postal code for work address (الرمز البريدي).
    /// </summary>
    [StringLength(5)]
    public string? WorkPostalCode { get; set; }

    /// <summary>
    /// Additional code for work address (الرمز الإضافي).
    /// </summary>
    [StringLength(4)]
    public string? WorkAdditionalCode { get; set; }

    #endregion

    #region Contact Info

    /// <summary>
    /// Mobile number.
    /// </summary>
    [StringLength(10)]
    [RegularExpression(@"^05\d{8}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 05")]
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [StringLength(255)]
    public string? Email { get; set; }

    #endregion

    #region Authorization Document

    /// <summary>
    /// Authorization/Power of Attorney number.
    /// </summary>
    [StringLength(50)]
    public string? AuthorizationNumber { get; set; }

    /// <summary>
    /// Authorization date.
    /// </summary>
    public DateTime? AuthorizationDate { get; set; }

    /// <summary>
    /// Authorization source/issuer.
    /// </summary>
    [StringLength(200)]
    public string? AuthorizationSource { get; set; }

    /// <summary>
    /// Authorization source type.
    /// </summary>
    [StringLength(50)]
    public string? AuthorizationSourceType { get; set; }

    #endregion

    #region Liquidator Data (مصفي - 6.3.12)

    /// <summary>
    /// Decision number (رقم القرار) for Liquidator.
    /// </summary>
    [StringLength(20)]
    public string? DecisionNumber { get; set; }

    /// <summary>
    /// Decision date (تاريخ القرار) for Liquidator.
    /// </summary>
    public DateTime? DecisionDate { get; set; }

    /// <summary>
    /// Decision source (مصدر القرار) for Liquidator.
    /// </summary>
    [StringLength(200)]
    public string? DecisionSource { get; set; }

    #endregion

    #region Guardian Data (ولي - 6.3.16)

    /// <summary>
    /// Deed number (رقم الصك) for Guardian.
    /// </summary>
    [StringLength(20)]
    public string? DeedNumber { get; set; }

    /// <summary>
    /// Deed date (تاريخ الصك) for Guardian.
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source (مصدر الصك) for Guardian.
    /// </summary>
    [StringLength(200)]
    public string? DeedSource { get; set; }

    /// <summary>
    /// Guardianship type for Guardian (ولي): طبيعية/مكتسبة.
    /// </summary>
    [StringLength(20)]
    public string? GuardianshipType { get; set; }

    #endregion
}
