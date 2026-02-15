using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// Data transfer object for taking an action on a case registration request.
/// Used in POST /api/case-requests/{requestId}/action
/// Supports actions: Register, SendToJudge, Reject, RequestCompletion, Complete
/// </summary>
public class TakeActionDTO
{
    /// <summary>
    /// Action to take on the request (required)
    /// Valid values: Register, SendToJudge, Reject, RequestCompletion, Complete
    /// </summary>
    [Required(ErrorMessage = "Action is required")]
    [RegularExpression(
        "^(Register|SendToJudge|Reject|RequestCompletion|Complete)$",
        ErrorMessage = "Invalid action. Valid actions: Register, SendToJudge, Reject, RequestCompletion, Complete")]
    public string Action { get; set; } = null!;

    /// <summary>
    /// Additional notes for the action (optional but required for Reject and RequestCompletion)
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// List of deficiencies for RequestCompletion action (optional)
    /// </summary>
    public List<RequestDeficiencyDTO>? Deficiencies { get; set; }

    /// <summary>
    /// Rejection reason (optional, but required if Action is Reject)
    /// </summary>
    [StringLength(2000, ErrorMessage = "Rejection reason cannot exceed 2000 characters")]
    public string? RejectionReason { get; set; }
}

