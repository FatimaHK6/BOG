using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Plaintiff;

/// <summary>
/// DTO for updating a plaintiff.
/// Only contains fields that can be updated (PlaintiffTypeId cannot be changed).
/// </summary>
public class PlaintiffUpdateDTO
{
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

    #region Personal Data (only for manual entry, not Absher data)

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
    /// Family name (اسم العائلة).
    /// </summary>
    [StringLength(100)]
    public string? FamilyName { get; set; }

    #endregion

    #region Business/Company Data

    /// <summary>
    /// Company name.
    /// </summary>
    [StringLength(200)]
    public string? CompanyName { get; set; }

    #endregion

    #region Additional Info

    /// <summary>
    /// Additional statement (بيان إضافي).
    /// </summary>
    [StringLength(4000)]
    public string? AdditionalStatement { get; set; }

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
}
