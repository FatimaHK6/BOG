namespace BOG.Integration.DTOs.Email;

/// <summary>
/// Result of an email sending operation.
/// </summary>
public class EmailResult
{
    /// <summary>
    /// Whether the email was sent successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Unique message ID from email provider.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Recipient email address.
    /// </summary>
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Error message if sending failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of sending.
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
