using BOG.Integration.DTOs.Email;
using BOG.Integration.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BOG.Integration.Services.Email;

/// <summary>
/// Mock email service for testing without actual SMTP server.
/// Logs all messages to console/logging infrastructure.
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EmailResult> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Validate email format
        if (!IsValidEmail(toEmail))
        {
            var result = new EmailResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid email format",
                RecipientEmail = toEmail
            };

            _logger.LogWarning("[MOCK EMAIL] Invalid format - {Email}", toEmail);
            return result;
        }

        // Simulate network delay
        await Task.Delay(100, cancellationToken);

        var messageId = $"MOCK-EMAIL-{Guid.NewGuid().ToString()[..8]}";

        _logger.LogInformation("[MOCK EMAIL] To: {Email} | Subject: {Subject} | MessageId: {MessageId}",
            toEmail, subject, messageId);

        return new EmailResult
        {
            IsSuccess = true,
            MessageId = messageId,
            RecipientEmail = toEmail,
            SentAt = DateTime.UtcNow
        };
    }

    public async Task<EmailResult> SendHtmlEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK EMAIL HTML] To: {Email} | Subject: {Subject}",
            toEmail, subject);

        return await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);
    }

    public async Task<EmailResult> SendTemplatedEmailAsync(string toEmail, string templateName, EmailTemplateParameters parameters, CancellationToken cancellationToken = default)
    {
        // Build email from template
        var (subject, body) = BuildEmailFromTemplate(templateName, parameters);

        _logger.LogInformation("[MOCK EMAIL TEMPLATE] Template: {Template} | To: {Email}",
            templateName, toEmail);

        return await SendHtmlEmailAsync(toEmail, subject, body, cancellationToken);
    }

    /// <summary>
    /// Validates email format using regex.
    /// </summary>
    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Builds email from template with parameter substitution.
    /// </summary>
    private static (string Subject, string Body) BuildEmailFromTemplate(string templateName, EmailTemplateParameters parameters)
    {
        return templateName.ToLower() switch
        {
            "request_submitted" => (
                Subject: "تأكيد استقبال طلب الدعوى",
                Body: BuildHtmlBody(
                    title: "استقبال الطلب",
                    content: $"السيد/ة {parameters.RecipientName},<br/>" +
                             $"تم استقبال طلبك برقم <strong>{parameters.RequestNumber}</strong> بنجاح.<br/>" +
                             $"سيتم الإجابة على طلبك في غضون 5 أيام عمل.<br/>" +
                             $"<strong>نظام إدارة الدعاوى</strong>"
                )
            ),

            "case_registered" => (
                Subject: "تم تسجيل الدعوى",
                Body: BuildHtmlBody(
                    title: "تسجيل الدعوى",
                    content: $"السيد/ة {parameters.RecipientName},<br/>" +
                             $"تم تسجيل دعواك برقم <strong>{parameters.CaseNumber}</strong> في {parameters.CourtName}.<br/>" +
                             $"رقم التسجيل: <strong>{parameters.RegistrationNumber}</strong><br/>" +
                             $"<strong>نظام إدارة الدعاوى</strong>"
                )
            ),

            "request_rejected" => (
                Subject: "رفض الطلب",
                Body: BuildHtmlBody(
                    title: "رفض الطلب",
                    content: $"السيد/ة {parameters.RecipientName},<br/>" +
                             $"تم رفض طلبك برقم <strong>{parameters.RequestNumber}</strong>.<br/>" +
                             $"السبب: {parameters.RejectionReason}<br/>" +
                             $"<strong>نظام إدارة الدعاوى</strong>"
                )
            ),

            "completion_required" => (
                Subject: "طلب استكمال المستندات",
                Body: BuildHtmlBody(
                    title: "استكمال المستندات",
                    content: $"السيد/ة {parameters.RecipientName},<br/>" +
                             $"طلبك برقم <strong>{parameters.RequestNumber}</strong> يحتاج لاستكمال مستندات.<br/>" +
                             $"آخر موعد للاستكمال: <strong>{parameters.Deadline:dd/MM/yyyy}</strong><br/>" +
                             $"<strong>نظام إدارة الدعاوى</strong>"
                )
            ),

            _ => (
                Subject: "إشعار من نظام إدارة الدعاوى",
                Body: parameters.CustomHtmlBody ?? BuildHtmlBody(
                    title: "إشعار",
                    content: $"السيد/ة {parameters.RecipientName},<br/>تم إرسال هذا الإشعار من نظام إدارة الدعاوى."
                )
            )
        };
    }

    /// <summary>
    /// Helper to build basic HTML email body.
    /// </summary>
    private static string BuildHtmlBody(string title, string content)
    {
        return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; }}
        .header {{ background-color: #004d99; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; }}
        .footer {{ background-color: #f5f5f5; padding: 10px; text-align: center; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>{title}</h2>
        </div>
        <div class='content'>
            {content}
        </div>
        <div class='footer'>
            <p>© 2026 وزارة العدل - نظام إدارة الدعاوى</p>
        </div>
    </div>
</body>
</html>";
    }
}
