using BOG.Integration.DTOs.Email;

namespace BOG.Integration.Interfaces;

/// <summary>
/// Service for sending email messages.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send a plain text email.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (plain text)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email sending result</returns>
    Task<EmailResult> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send an HTML email.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">Email body (HTML)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email sending result</returns>
    Task<EmailResult> SendHtmlEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a templated email with parameter substitution.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="templateName">Email template name</param>
    /// <param name="parameters">Template parameters for substitution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email sending result</returns>
    Task<EmailResult> SendTemplatedEmailAsync(string toEmail, string templateName, EmailTemplateParameters parameters, CancellationToken cancellationToken = default);
}
