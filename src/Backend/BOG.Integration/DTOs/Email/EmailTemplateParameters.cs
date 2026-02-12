namespace BOG.Integration.DTOs.Email;

/// <summary>
/// Parameters for email template substitution.
/// </summary>
public class EmailTemplateParameters
{
    /// <summary>
    /// Recipient name for personalization.
    /// </summary>
    public string? RecipientName { get; set; }

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
    /// Registration number for template substitution.
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Custom HTML body (overrides template if provided).
    /// </summary>
    public string? CustomHtmlBody { get; set; }
}
