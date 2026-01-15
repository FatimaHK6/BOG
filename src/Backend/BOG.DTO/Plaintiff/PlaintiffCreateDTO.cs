using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Plaintiff;

/// <summary>
/// DTO for creating a new plaintiff.
/// Fields are type-specific based on PlaintiffTypeId.
/// </summary>
public class PlaintiffCreateDTO
{
    /// <summary>
    /// Type of plaintiff (required): 1-Individual, 2-IndividualNoId, 3-BusinessOwner,
    /// 4-RegisteredCompany, 5-UnregisteredCompany, 6-GovernmentAgency, 7-NGO, 8-Waqf.
    /// </summary>
    [Required(ErrorMessage = "نوع المدعي مطلوب")]
    public int PlaintiffTypeId { get; set; }

    #region Personal Data (for Individual types: 1, 2)

    /// <summary>
    /// Identity type ID: 1-National ID, 2-Resident ID, 3-Passport.
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number (رقم الهوية) - 10 digits for National/Resident, up to 20 for Passport.
    /// </summary>
    [StringLength(20, ErrorMessage = "رقم الهوية يجب ألا يتجاوز 20 حرف")]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// First name (الاسم الأول).
    /// </summary>
    [StringLength(100, ErrorMessage = "الاسم الأول يجب ألا يتجاوز 100 حرف")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Father's name (اسم الأب).
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم الأب يجب ألا يتجاوز 100 حرف")]
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather's name (اسم الجد).
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم الجد يجب ألا يتجاوز 100 حرف")]
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Family name (اسم العائلة).
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم العائلة يجب ألا يتجاوز 100 حرف")]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gender (ذكر/أنثى).
    /// </summary>
    [StringLength(10)]
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality ID.
    /// </summary>
    public int? NationalityId { get; set; }

    #endregion

    #region Contact Info

    /// <summary>
    /// Mobile number (10 digits, starts with 05).
    /// </summary>
    [StringLength(10, ErrorMessage = "رقم الجوال يجب أن يكون 10 أرقام")]
    [RegularExpression(@"^05\d{8}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 05 ويتكون من 10 أرقام")]
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [StringLength(255)]
    public string? Email { get; set; }

    #endregion

    #region Business/Company Data (for types: 3, 4, 5)

    /// <summary>
    /// Commercial registration number (10 digits).
    /// </summary>
    [StringLength(10, ErrorMessage = "رقم السجل التجاري يجب أن يكون 10 أرقام")]
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    [StringLength(200, ErrorMessage = "اسم الشركة يجب ألا يتجاوز 200 حرف")]
    public string? CompanyName { get; set; }

    #endregion

    #region Government Agency Data (for type: 6)

    /// <summary>
    /// Government agency ID.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Additional statement (بيان إضافي).
    /// </summary>
    [StringLength(4000, ErrorMessage = "البيان الإضافي يجب ألا يتجاوز 4000 حرف")]
    public string? AdditionalStatement { get; set; }

    #endregion

    #region Society/NGO Data (for type: 7)

    /// <summary>
    /// License number.
    /// </summary>
    [StringLength(50)]
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// License source/issuer.
    /// </summary>
    [StringLength(200)]
    public string? LicenseSource { get; set; }

    /// <summary>
    /// License date.
    /// </summary>
    public DateTime? LicenseDate { get; set; }

    #endregion

    #region Waqf Data (for type: 8)

    /// <summary>
    /// Court deed number.
    /// </summary>
    [StringLength(50)]
    public string? CourtDeedNumber { get; set; }

    /// <summary>
    /// Deed date.
    /// </summary>
    public DateTime? DeedDate { get; set; }

    /// <summary>
    /// Deed source/issuer.
    /// </summary>
    [StringLength(200)]
    public string? DeedSource { get; set; }

    /// <summary>
    /// Waqf oversight type (خاصة/حكومية).
    /// </summary>
    [StringLength(20)]
    public string? WaqfOversightType { get; set; }

    #endregion
}
