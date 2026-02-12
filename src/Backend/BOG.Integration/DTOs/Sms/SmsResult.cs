namespace BOG.Integration.DTOs.Sms;

/// <summary>
/// Result of an SMS sending operation.
/// </summary>
public class SmsResult
{
    /// <summary>
    /// Whether the SMS was sent successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Unique message ID from SMS provider.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Mobile number the SMS was sent to.
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Error message if sending failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of sending.
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
