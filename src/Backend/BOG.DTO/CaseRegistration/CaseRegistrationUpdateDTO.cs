using System.ComponentModel.DataAnnotations;
using BOG.DTO.Validation;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// Data transfer object for updating a case registration request.
/// Used in PUT /api/case-requests/{requestId}
/// All fields are optional - only provided fields will be updated
/// Can only update requests in Draft or PendingCompletion state
/// </summary>
public class CaseRegistrationUpdateDTO
{
    /// <summary>
    /// Case subject/title (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "موضوع الدعوى لا يمكن أن يتجاوز 4000 حرف")]
    public string? Subject { get; set; }

    /// <summary>
    /// Evidence description (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "بيانات الإثبات لا يمكن أن تتجاوز 4000 حرف")]
    public string? Evidence { get; set; }

    /// <summary>
    /// Court ID (optional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "معرف المحكمة غير صالح")]
    public int? CourtId { get; set; }

    /// <summary>
    /// Case type ID (optional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "نوع الدعوى غير صالح")]
    public int? CaseTypeId { get; set; }

    /// <summary>
    /// Additional case notes (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 4000 حرف")]
    public string? Notes { get; set; }

    /// <summary>
    /// Classification IDs for the case (optional)
    /// </summary>
    public List<int>? ClassificationIds { get; set; }

    /// <summary>
    /// Primary mobile number for contact (optional, must match Saudi format: 05XXXXXXXX)
    /// </summary>
    [SaudiMobile(ErrorMessage = "رقم الجوال الأساسي يجب أن يكون 10 أرقام ويبدأ بـ 05")]
    public string? PrimaryMobile { get; set; }

    /// <summary>
    /// Secondary mobile number for contact (optional, must match Saudi format: 05XXXXXXXX)
    /// </summary>
    [SaudiMobile(ErrorMessage = "رقم الجوال الثانوي يجب أن يكون 10 أرقام ويبدأ بـ 05")]
    public string? SecondaryMobile { get; set; }

    /// <summary>
    /// Email address for contact (optional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Whether to save as draft (default: true)
    /// If true, status remains/becomes Draft
    /// If false, business logic determines next status
    /// </summary>
    public bool SaveAsDraft { get; set; } = true;
}
