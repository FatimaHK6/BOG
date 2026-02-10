using BOG.Integration.DTOs.Email;
using BOG.Integration.Services.Email;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BOG.Tests.Integration;

/// <summary>
/// Unit tests for MockEmailService.
/// </summary>
public class MockEmailServiceTests
{
    private readonly ILogger<MockEmailService> _mockLogger;

    public MockEmailServiceTests()
    {
        _mockLogger = new MockLogger<MockEmailService>();
    }

    #region SendEmailAsync Tests

    [Fact]
    public async Task SendEmailAsync_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var toEmail = "test@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        var result = await service.SendEmailAsync(toEmail, subject, body);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.MessageId);
        Assert.Equal(toEmail, result.RecipientEmail);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData("invalid-email")]       // No @ symbol
    [InlineData("invalid@")]            // No domain
    [InlineData("@example.com")]        // No local part
    [InlineData("")]                    // Empty
    [InlineData(null)]                  // Null
    public async Task SendEmailAsync_WithInvalidFormat_ReturnsFail(string toEmail)
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);

        // Act
        var result = await service.SendEmailAsync(toEmail!, "Subject", "Body");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task SendEmailAsync_GeneratesUniqueMessageIds()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var toEmail = "test@example.com";

        // Act
        var result1 = await service.SendEmailAsync(toEmail, "Subject 1", "Body 1");
        var result2 = await service.SendEmailAsync(toEmail, "Subject 2", "Body 2");

        // Assert
        Assert.NotEqual(result1.MessageId, result2.MessageId);
    }

    #endregion

    #region SendHtmlEmailAsync Tests

    [Fact]
    public async Task SendHtmlEmailAsync_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var toEmail = "test@example.com";
        var subject = "Test Subject";
        var htmlBody = "<html><body><h1>Test</h1></body></html>";

        // Act
        var result = await service.SendHtmlEmailAsync(toEmail, subject, htmlBody);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.MessageId);
    }

    #endregion

    #region SendTemplatedEmailAsync Tests

    [Fact]
    public async Task SendTemplatedEmailAsync_RequestSubmitted_ReturnsSuccess()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var parameters = new EmailTemplateParameters
        {
            RecipientName = "أحمد محمد",
            RequestNumber = "12345"
        };

        // Act
        var result = await service.SendTemplatedEmailAsync("test@example.com", "request_submitted", parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_CaseRegistered_ReturnsSuccess()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var parameters = new EmailTemplateParameters
        {
            RecipientName = "أحمد محمد",
            CaseNumber = "CASE-2024-001",
            RegistrationNumber = "REG-001",
            CourtName = "محكمة الاستئناف"
        };

        // Act
        var result = await service.SendTemplatedEmailAsync("test@example.com", "case_registered", parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_RequestRejected_ReturnsSuccess()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var parameters = new EmailTemplateParameters
        {
            RecipientName = "أحمد محمد",
            RequestNumber = "12345",
            RejectionReason = "مستندات ناقصة"
        };

        // Act
        var result = await service.SendTemplatedEmailAsync("test@example.com", "request_rejected", parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_CompletionRequired_ReturnsSuccess()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var deadline = DateTime.UtcNow.AddDays(30);
        var parameters = new EmailTemplateParameters
        {
            RecipientName = "أحمد محمد",
            RequestNumber = "12345",
            Deadline = deadline
        };

        // Act
        var result = await service.SendTemplatedEmailAsync("test@example.com", "completion_required", parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Result Tests

    [Fact]
    public async Task SendEmailAsync_SetsCurrentTimestamp()
    {
        // Arrange
        var service = new MockEmailService(_mockLogger);
        var beforeTime = DateTime.UtcNow;

        // Act
        var result = await service.SendEmailAsync("test@example.com", "Subject", "Body");
        var afterTime = DateTime.UtcNow;

        // Assert
        Assert.True(result.SentAt >= beforeTime);
        Assert.True(result.SentAt <= afterTime);
    }

    #endregion
}
