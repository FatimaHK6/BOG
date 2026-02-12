using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// DTO for making final decision on case registration request.
/// Used in POST /api/case-requests/{id}/complete
/// </summary>
public class RequestDecisionDTO
{
    /// <summary>
    /// Decision type
    /// </summary>
    [Required(ErrorMessage = "نوع القرار مطلوب")]
    [RegularExpression("^(Register|SendToJudge|Reject|RequestCompletion)$",
        ErrorMessage = "نوع قرار غير صالح")]
    public string DecisionType { get; set; } = "";

    /// <summary>
    /// Case type ID (ALWAYS REQUIRED for all decisions)
    /// Valid values: 1 (إداري) or 2 (تأديبي)
    /// </summary>
    [Required(ErrorMessage = "نوع الدعوى مطلوب")]
    [Range(1, 2, ErrorMessage = "نوع دعوى غير صالح")]
    public int CaseTypeId { get; set; } = 1;

    /// <summary>
    /// Notes/remarks (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 4000 حرف")]
    public string? Notes { get; set; }

    /// <summary>
    /// Deficiencies list (for RequestCompletion decision)
    /// </summary>
    public List<string>? Deficiencies { get; set; }
}
