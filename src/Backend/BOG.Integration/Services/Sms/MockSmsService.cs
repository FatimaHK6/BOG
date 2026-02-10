using BOG.Integration.DTOs.Sms;
using BOG.Integration.Enums;
using BOG.Integration.Interfaces;
using Microsoft.Extensions.Logging;

namespace BOG.Integration.Services.Sms;

/// <summary>
/// Mock SMS service for testing without actual SMS provider.
/// Logs all messages to console/logging infrastructure.
/// </summary>
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SmsResult> SendSmsAsync(string mobileNumber, string message, CancellationToken cancellationToken = default)
    {
        // Validate Saudi mobile number format (05XXXXXXXXXX - 10 digits)
        if (!IsValidSaudiMobileNumber(mobileNumber))
        {
            var result = new SmsResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid Saudi mobile number format. Expected format: 05XXXXXXXXXX",
                MobileNumber = mobileNumber
            };

            _logger.LogWarning("[MOCK SMS] Invalid format - {Mobile}", mobileNumber);
            return result;
        }

        // Simulate network delay
        await Task.Delay(100, cancellationToken);

        var messageId = $"MOCK-SMS-{Guid.NewGuid().ToString()[..8]}";

        _logger.LogInformation("[MOCK SMS] To: {Mobile} | MessageId: {MessageId} | Message: {Message}",
            mobileNumber, messageId, message);

        return new SmsResult
        {
            IsSuccess = true,
            MessageId = messageId,
            MobileNumber = mobileNumber,
            SentAt = DateTime.UtcNow
        };
    }

    public async Task<SmsResult> SendTemplatedSmsAsync(string mobileNumber, SmsTemplate template, SmsTemplateParameters parameters, CancellationToken cancellationToken = default)
    {
        // Build message from template
        var message = BuildMessageFromTemplate(template, parameters);

        _logger.LogInformation("[MOCK SMS TEMPLATE] Template: {Template} | To: {Mobile}",
            template, mobileNumber);

        return await SendSmsAsync(mobileNumber, message, cancellationToken);
    }

    /// <summary>
    /// Validates Saudi mobile number format (05XXXXXXXXXX - 10 digits starting with 05).
    /// </summary>
    private static bool IsValidSaudiMobileNumber(string? mobileNumber)
    {
        return !string.IsNullOrWhiteSpace(mobileNumber)
            && mobileNumber.Length == 10
            && mobileNumber.StartsWith("05")
            && mobileNumber.All(char.IsDigit);
    }

    /// <summary>
    /// Builds SMS message from template with parameter substitution.
    /// </summary>
    private static string BuildMessageFromTemplate(SmsTemplate template, SmsTemplateParameters parameters)
    {
        return template switch
        {
            SmsTemplate.REQUEST_SUBMITTED => $"طلبك رقم {parameters.RequestNumber} تم استقباله بنجاح. شكراً لاستخدام نظام الدعاوى.",

            SmsTemplate.REQUEST_REGISTERED => $"تم تسجيل الدعوى برقم {parameters.CaseNumber} في محكمة {parameters.CourtName}.",

            SmsTemplate.REQUEST_REJECTED => $"تم رفض طلبك برقم {parameters.RequestNumber}. السبب: {parameters.RejectionReason}",

            SmsTemplate.COMPLETION_REQUIRED => $"طلبك رقم {parameters.RequestNumber} يحتاج لاستكمال مستندات قبل {parameters.Deadline:dd/MM/yyyy}.",

            SmsTemplate.DEFICIENCY_REMINDER => $"تذكير: آخر موعد لاستكمال مستندات طلبك رقم {parameters.RequestNumber} هو {parameters.Deadline:dd/MM/yyyy}.",

            SmsTemplate.AUTO_REJECTED => $"طلبك رقم {parameters.RequestNumber} تم رفضه تلقائياً بسبب انتهاء مدة الاستكمال.",

            _ => parameters.CustomMessage ?? "رسالة من نظام إدارة الدعاوى"
        };
    }
}
