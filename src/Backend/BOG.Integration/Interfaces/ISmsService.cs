using BOG.Integration.DTOs.Sms;
using BOG.Integration.Enums;

namespace BOG.Integration.Interfaces;

/// <summary>
/// Service for sending SMS messages.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send a plain text SMS message.
    /// </summary>
    /// <param name="mobileNumber">Recipient mobile number (Saudi format: 05XXXXXXXXX)</param>
    /// <param name="message">Message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SMS sending result</returns>
    Task<SmsResult> SendSmsAsync(string mobileNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a templated SMS message with parameter substitution.
    /// </summary>
    /// <param name="mobileNumber">Recipient mobile number (Saudi format: 05XXXXXXXXX)</param>
    /// <param name="template">SMS template type</param>
    /// <param name="parameters">Template parameters for substitution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SMS sending result</returns>
    Task<SmsResult> SendTemplatedSmsAsync(string mobileNumber, SmsTemplate template, SmsTemplateParameters parameters, CancellationToken cancellationToken = default);
}
