using BOG.Integration.DTOs.Sms;
using BOG.Integration.Enums;
using BOG.Integration.Services.Sms;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BOG.Tests.Integration;

/// <summary>
/// Unit tests for MockSmsService.
/// </summary>
public class MockSmsServiceTests
{
    private readonly ILogger<MockSmsService> _mockLogger;

    public MockSmsServiceTests()
    {
        // Create a mock logger
        _mockLogger = new MockLogger<MockSmsService>();
    }

    #region SendSmsAsync Tests

    [Fact]
    public async Task SendSmsAsync_WithValidSaudiNumber_ReturnsSuccess()
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);
        var mobileNumber = "0501234567";
        var message = "Test message";

        // Act
        var result = await service.SendSmsAsync(mobileNumber, message);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.MessageId);
        Assert.Equal(mobileNumber, result.MobileNumber);
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData("1234567890")]      // Doesn't start with 05
    [InlineData("05123456")]        // Too short
    [InlineData("051234567890")]    // Too long
    [InlineData("+966501234567")]   // International format
    [InlineData("")]                // Empty
    [InlineData(null)]              // Null
    public async Task SendSmsAsync_WithInvalidFormat_ReturnsFail(string mobileNumber)
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);

        // Act
        var result = await service.SendSmsAsync(mobileNumber!, "Test");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("format", result.ErrorMessage.ToLower());
    }

    [Fact]
    public async Task SendSmsAsync_GeneratesUniqueMessageIds()
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);
        var mobileNumber = "0501234567";

        // Act
        var result1 = await service.SendSmsAsync(mobileNumber, "Message 1");
        var result2 = await service.SendSmsAsync(mobileNumber, "Message 2");

        // Assert
        Assert.NotEqual(result1.MessageId, result2.MessageId);
    }

    #endregion

    #region SendTemplatedSmsAsync Tests

    [Fact]
    public async Task SendTemplatedSmsAsync_RequestSubmitted_SendsCorrectMessage()
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);
        var parameters = new SmsTemplateParameters { RequestNumber = "12345" };

        // Act
        var result = await service.SendTemplatedSmsAsync("0501234567", SmsTemplate.REQUEST_SUBMITTED, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendTemplatedSmsAsync_RequestRejected_IncludesReason()
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);
        var parameters = new SmsTemplateParameters
        {
            RequestNumber = "12345",
            RejectionReason = "Missing documents"
        };

        // Act
        var result = await service.SendTemplatedSmsAsync("0501234567", SmsTemplate.REQUEST_REJECTED, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendTemplatedSmsAsync_CompletionRequired_IncludesDeadline()
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);
        var deadline = DateTime.UtcNow.AddDays(30);
        var parameters = new SmsTemplateParameters
        {
            RequestNumber = "12345",
            Deadline = deadline
        };

        // Act
        var result = await service.SendTemplatedSmsAsync("0501234567", SmsTemplate.COMPLETION_REQUIRED, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendTemplatedSmsAsync_AutoRejected_SendsMessage()
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);
        var parameters = new SmsTemplateParameters { RequestNumber = "12345" };

        // Act
        var result = await service.SendTemplatedSmsAsync("0501234567", SmsTemplate.AUTO_REJECTED, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Result Tests

    [Fact]
    public async Task SendSmsAsync_SetsCurrentTimestamp()
    {
        // Arrange
        var service = new MockSmsService(_mockLogger);
        var beforeTime = DateTime.UtcNow;

        // Act
        var result = await service.SendSmsAsync("0501234567", "Test");
        var afterTime = DateTime.UtcNow;

        // Assert
        Assert.True(result.SentAt >= beforeTime);
        Assert.True(result.SentAt <= afterTime);
    }

    #endregion
}

/// <summary>
/// Mock implementation of ILogger for testing.
/// </summary>
public class MockLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
