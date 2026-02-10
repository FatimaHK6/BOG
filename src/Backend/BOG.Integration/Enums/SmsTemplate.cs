namespace BOG.Integration.Enums;

/// <summary>
/// SMS message templates for different case registration events.
/// </summary>
public enum SmsTemplate
{
    /// <summary>
    /// Request submitted successfully
    /// </summary>
    REQUEST_SUBMITTED = 1,

    /// <summary>
    /// Case registered successfully
    /// </summary>
    REQUEST_REGISTERED = 2,

    /// <summary>
    /// Request rejected
    /// </summary>
    REQUEST_REJECTED = 3,

    /// <summary>
    /// Completion of deficiencies required
    /// </summary>
    COMPLETION_REQUIRED = 4,

    /// <summary>
    /// Reminder about deficiency completion
    /// </summary>
    DEFICIENCY_REMINDER = 5,

    /// <summary>
    /// Request auto-rejected after deadline
    /// </summary>
    AUTO_REJECTED = 6
}
