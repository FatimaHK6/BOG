namespace BOG.Integration.DTOs.Sms;

/// <summary>
/// Parameters for SMS template substitution.
/// </summary>
public class SmsTemplateParameters
{
    /// <summary>
    /// Request number for template substitution.
    /// </summary>
    public string? RequestNumber { get; set; }

    /// <summary>
    /// Case number for template substitution.
    /// </summary>
    public string? CaseNumber { get; set; }

    /// <summary>
    /// Court name for template substitution.
    /// </summary>
    public string? CourtName { get; set; }

    /// <summary>
    /// Rejection reason for template substitution.
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Deadline date for template substitution.
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Custom message (overrides template if provided).
    /// </summary>
    public string? CustomMessage { get; set; }
}
