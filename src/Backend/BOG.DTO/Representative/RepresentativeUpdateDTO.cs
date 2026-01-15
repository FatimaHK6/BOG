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
}
