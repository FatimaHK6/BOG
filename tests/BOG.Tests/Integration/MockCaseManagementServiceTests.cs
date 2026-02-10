using BOG.Integration.Configuration;
using BOG.Integration.DTOs.CaseManagement;
using BOG.Integration.Services.CaseManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace BOG.Tests.Integration;

/// <summary>
/// Unit tests for MockCaseManagementService.
/// </summary>
public class MockCaseManagementServiceTests
{
    private readonly ILogger<MockCaseManagementService> _mockLogger;
    private readonly IOptions<CaseManagementSettings> _settings;

    public MockCaseManagementServiceTests()
    {
        _mockLogger = new MockLogger<MockCaseManagementService>();
        var options = Options.Create(new CaseManagementSettings
        {
            UseMock = true,
            MockCaseNumberPrefix = "TEST-CASE-",
            MockRegistrationNumberPrefix = "TEST-REG-"
        });
        _settings = options;
    }

    #region RegisterCaseAsync Tests

    [Fact]
    public async Task RegisterCaseAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);
        var data = new CaseRegistrationData
        {
            RequestId = 1,
            CourtId = 1,
            Subject = "Case Subject",
            Evidence = "Evidence description",
            Plaintiffs = new List<CasePartyData>
            {
                new() { FullName = "محمد أحمد", IdentityNumber = "1234567890" }
            },
            Defendants = new List<CasePartyData>
            {
                new() { FullName = "علي سالم", IdentityNumber = "0987654321" }
            }
        };

        // Act
        var result = await service.RegisterCaseAsync(data);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.CaseNumber);
        Assert.NotNull(result.RegistrationNumber);
        Assert.StartsWith("TEST-CASE-", result.CaseNumber);
        Assert.StartsWith("TEST-REG-", result.RegistrationNumber);
    }

    [Fact]
    public async Task RegisterCaseAsync_WithNullData_ReturnsFail()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);

        // Act
        var result = await service.RegisterCaseAsync(null!);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterCaseAsync_WithoutSubject_ReturnsFail()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);
        var data = new CaseRegistrationData
        {
            RequestId = 1,
            CourtId = 1,
            Subject = null,
            Plaintiffs = new List<CasePartyData> { new() { FullName = "محمد" } },
            Defendants = new List<CasePartyData> { new() { FullName = "علي" } }
        };

        // Act
        var result = await service.RegisterCaseAsync(data);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("subject", result.ErrorMessage!.ToLower());
    }

    [Fact]
    public async Task RegisterCaseAsync_WithoutPlaintiffs_ReturnsFail()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);
        var data = new CaseRegistrationData
        {
            RequestId = 1,
            CourtId = 1,
            Subject = "Case",
            Plaintiffs = new List<CasePartyData>(),
            Defendants = new List<CasePartyData> { new() { FullName = "علي" } }
        };

        // Act
        var result = await service.RegisterCaseAsync(data);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("plaintiff", result.ErrorMessage!.ToLower());
    }

    [Fact]
    public async Task RegisterCaseAsync_WithoutDefendants_ReturnsFail()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);
        var data = new CaseRegistrationData
        {
            RequestId = 1,
            CourtId = 1,
            Subject = "Case",
            Plaintiffs = new List<CasePartyData> { new() { FullName = "محمد" } },
            Defendants = new List<CasePartyData>()
        };

        // Act
        var result = await service.RegisterCaseAsync(data);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("defendant", result.ErrorMessage!.ToLower());
    }

    [Fact]
    public async Task RegisterCaseAsync_GeneratesUniqueNumbers()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);
        var data = new CaseRegistrationData
        {
            RequestId = 1,
            CourtId = 1,
            Subject = "Case",
            Plaintiffs = new List<CasePartyData> { new() { FullName = "محمد" } },
            Defendants = new List<CasePartyData> { new() { FullName = "علي" } }
        };

        // Act
        var result1 = await service.RegisterCaseAsync(data);
        var result2 = await service.RegisterCaseAsync(data);

        // Assert
        Assert.NotEqual(result1.CaseNumber, result2.CaseNumber);
        Assert.NotEqual(result1.RegistrationNumber, result2.RegistrationNumber);
    }

    #endregion

    #region GetCaseStatusAsync Tests

    [Fact]
    public async Task GetCaseStatusAsync_WithValidCaseNumber_ReturnsSuccess()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);
        var caseNumber = "TEST-CASE-1001";

        // Act
        var result = await service.GetCaseStatusAsync(caseNumber);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(caseNumber, result.CaseNumber);
        Assert.NotNull(result.Status);
    }

    [Fact]
    public async Task GetCaseStatusAsync_WithNullCaseNumber_ReturnsFail()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);

        // Act
        var result = await service.GetCaseStatusAsync(null!);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetCaseStatusAsync_WithEmptyCaseNumber_ReturnsFail()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);

        // Act
        var result = await service.GetCaseStatusAsync("");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetCaseStatusAsync_ReturnsNextHearingDate()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);

        // Act
        var result = await service.GetCaseStatusAsync("TEST-CASE-1001");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.NextHearingDate);
        Assert.True(result.NextHearingDate > DateTime.UtcNow);
    }

    #endregion

    #region Result Validation Tests

    [Fact]
    public async Task RegisterCaseAsync_SetsRegistrationDate()
    {
        // Arrange
        var service = new MockCaseManagementService(_mockLogger, _settings);
        var beforeTime = DateTime.UtcNow;
        var data = new CaseRegistrationData
        {
            RequestId = 1,
            CourtId = 1,
            Subject = "Case",
            Plaintiffs = new List<CasePartyData> { new() { FullName = "محمد" } },
            Defendants = new List<CasePartyData> { new() { FullName = "علي" } }
        };

        // Act
        var result = await service.RegisterCaseAsync(data);
        var afterTime = DateTime.UtcNow;

        // Assert
        Assert.True(result.RegistrationDate >= beforeTime);
        Assert.True(result.RegistrationDate <= afterTime);
    }

    #endregion
}
