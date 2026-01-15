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

    #region Guardian Data

    /// <summary>
    /// Guardianship type for Guardian (ولي): طبيعية/مكتسبة.
    /// </summary>
    [StringLength(20)]
    public string? GuardianshipType { get; set; }

    #endregion
}
