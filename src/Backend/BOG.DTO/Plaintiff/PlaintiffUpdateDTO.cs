using System.ComponentModel.DataAnnotations;
using BOG.DTO.Common;

namespace BOG.DTO.Plaintiff;

/// <summary>
/// DTO for updating a plaintiff.
/// </summary>
public class PlaintiffUpdateDTO
{
    #region Plaintiff Type

    /// <summary>
    /// Plaintiff type ID (نوع المدعي).
    /// </summary>
    public int? PlaintiffTypeId { get; set; }

    #endregion

    #region Identity Data (only for manual entry, not Absher data)

    /// <summary>
    /// Identity type ID.
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number.
    /// </summary>
    [StringLength(10)]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Identity issue date.
    /// </summary>
    public DateTime? IdentityIssueDate { get; set; }

    /// <summary>
    /// Identity expiry date.
    /// </summary>
    public DateTime? IdentityExpiryDate { get; set; }

    /// <summary>
    /// Document number (for Type 2 - Individual without ID).
    /// </summary>
    [StringLength(20)]
    public string? DocumentNumber { get; set; }

    #endregion

    #region Personal Data

    /// <summary>
    /// First name (الاسم الأول).
    /// </summary>
    [StringLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Father's name (اسم الأب).
    /// </summary>
    [StringLength(100)]
    public string? FatherName { get; set; }

    /// <summary>
    /// Grandfather's name (اسم الجد).
    /// </summary>
    [StringLength(100)]
    public string? GrandfatherName { get; set; }

    /// <summary>
    /// Clan name (اسم الفخذ).
    /// </summary>
    [StringLength(100)]
    public string? ClanName { get; set; }

    /// <summary>
    /// Family name (اسم العائلة).
    /// </summary>
    [StringLength(100)]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Gender (ذكر/أنثى).
    /// </summary>
    [StringLength(10)]
    public string? Gender { get; set; }

    /// <summary>
    /// Birth date.
    /// </summary>
    public DateTime? BirthDate { get; set; }

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

    #region Employment Data

    /// <summary>
    /// Employment status ID.
    /// </summary>
    public int? EmploymentStatusId { get; set; }

    /// <summary>
    /// Employer name.
    /// </summary>
    [StringLength(200)]
    public string? Employer { get; set; }

    /// <summary>
    /// Profession/occupation.
    /// </summary>
    [StringLength(100)]
    public string? Profession { get; set; }

    #endregion

    #region Business/Company Data

    /// <summary>
    /// Company name.
    /// </summary>
    [StringLength(200)]
    public string? CompanyName { get; set; }

    /// <summary>
    /// Commercial registration number.
    /// </summary>
    [StringLength(20)]
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// License number.
    /// </summary>
    [StringLength(20)]
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// License source ID.
    /// </summary>
    public int? LicenseSourceId { get; set; }

    /// <summary>
    /// Government agency ID.
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Headquarters (المقر).
    /// </summary>
    [StringLength(200)]
    public string? Headquarters { get; set; }

    /// <summary>
    /// Waqf oversight type.
    /// </summary>
    [StringLength(50)]
    public string? WaqfOversightType { get; set; }

    #endregion

    #region Additional Info

    /// <summary>
    /// Additional statement (بيان إضافي).
    /// </summary>
    [StringLength(4000)]
    public string? AdditionalStatement { get; set; }

    /// <summary>
    /// Whether this plaintiff is the applicant.
    /// </summary>
    public bool? IsApplicant { get; set; }

    /// <summary>
    /// Whether this plaintiff is saved as a draft (حفظ كمسودة).
    /// When true, validation rules are relaxed.
    /// </summary>
    public bool? IsDraft { get; set; }

    #endregion

    #region Addresses

    /// <summary>
    /// Residence address (عنوان السكن) - for individuals.
    /// </summary>
    public AddressCreateDTO? ResidenceAddress { get; set; }

    /// <summary>
    /// Work address (عنوان العمل) - for individuals.
    /// </summary>
    public AddressCreateDTO? WorkAddress { get; set; }

    /// <summary>
    /// Business/Institution address (عنوان المؤسسة) - for type 6 (Business Owner).
    /// </summary>
    public AddressCreateDTO? BusinessAddress { get; set; }

    /// <summary>
    /// Company address (عنوان الشركة) - for type 2 (Registered Company).
    /// </summary>
    public AddressCreateDTO? CompanyAddress { get; set; }

    /// <summary>
    /// NGO address (عنوان الجمعية) - for type 4 (NGO).
    /// </summary>
    public AddressCreateDTO? NGOAddress { get; set; }

    /// <summary>
    /// Waqf address (عنوان الوقف) - for type 5 (Waqf).
    /// </summary>
    public AddressCreateDTO? WaqfAddress { get; set; }

    /// <summary>
    /// Selected/Custom address for correspondence (العنوان المختار).
    /// </summary>
    public AddressCreateDTO? SelectedAddress { get; set; }

    #endregion
}
