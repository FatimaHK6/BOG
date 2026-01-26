using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Representative;

/// <summary>
/// DTO for creating a new representative.
/// </summary>
public class RepresentativeCreateDTO
{
    /// <summary>
    /// Representative type ID (required): 1-Lawyer, 2-Liquidator, 3-BankruptcyTrustee,
    /// 4-JudicialCustodian, 5-CompanyRep, 6-Guardian, 7-GovRep, 8-Conservator, 9-WaqfInspector.
    /// </summary>
    [Required(ErrorMessage = "نوع الممثل مطلوب")]
    public int RepresentativeTypeId { get; set; }

    #region Personal Data

    /// <summary>
    /// Identity type ID (required): 1-National ID, 2-Resident ID, 3-Passport.
    /// </summary>
    [Required(ErrorMessage = "نوع الهوية مطلوب")]
    public int IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number (required).
    /// </summary>
    [Required(ErrorMessage = "رقم الهوية مطلوب")]
    [StringLength(20, ErrorMessage = "رقم الهوية يجب ألا يتجاوز 20 حرف")]
    public string IdentityNumber { get; set; } = null!;

    /// <summary>
    /// First name (required).
    /// </summary>
    [Required(ErrorMessage = "الاسم الأول مطلوب")]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

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
    /// Family name (required).
    /// </summary>
    [Required(ErrorMessage = "اسم العائلة مطلوب")]
    [StringLength(100)]
    public string FamilyName { get; set; } = null!;

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

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

    /// <summary>
    /// Clan name (اسم الفخذ).
    /// </summary>
    [StringLength(100)]
    public string? ClanName { get; set; }

    #endregion

    #region Residence Address (عنوان السكن - 6.3.2)

    /// <summary>
    /// Region ID for residence address (المنطقة) - Required per BC04.
    /// </summary>
    [Required(ErrorMessage = "المنطقة مطلوبة")]
    public int? ResidenceRegionId { get; set; }

    /// <summary>
    /// City ID for residence address (المدينة) - Required per BC04.
    /// </summary>
    [Required(ErrorMessage = "المدينة مطلوبة")]
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
    [RegularExpression(@"^\d{5}$", ErrorMessage = "الرمز البريدي يجب أن يكون 5 أرقام")]
    public string? ResidencePostalCode { get; set; }

    /// <summary>
    /// Additional code for residence address (الرمز الإضافي).
    /// </summary>
    [StringLength(4)]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "الرمز الإضافي يجب أن يكون 4 أرقام")]
    public string? ResidenceAdditionalCode { get; set; }

    #endregion

    #region Employment Data (بيانات العمل)

    /// <summary>
    /// Employment status (حالة العمل): government/private/unemployed.
    /// </summary>
    [Required(ErrorMessage = "حالة العمل مطلوبة")]
    [StringLength(20)]
    public string? EmploymentStatus { get; set; }

    /// <summary>
    /// Employer name (جهة العمل) - conditional BC01: required if government/private.
    /// </summary>
    [StringLength(200)]
    public string? Employer { get; set; }

    /// <summary>
    /// Profession (المهنة).
    /// </summary>
    [Required(ErrorMessage = "المهنة مطلوبة")]
    [StringLength(200)]
    public string? Profession { get; set; }

    #endregion

    #region Work Address (عنوان العمل - 6.3.2) - Conditional BC02

    /// <summary>
    /// Region ID for work address (المنطقة) - conditional BC02.
    /// </summary>
    public int? WorkRegionId { get; set; }

    /// <summary>
    /// City ID for work address (المدينة) - conditional BC02.
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
    /// Authorization/Power of Attorney number (رقم الوكالة/القرار/الصك).
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
    /// Authorization source type (كتابة عدل/محكمة/وزارة خارجية).
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
