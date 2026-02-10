using BOG.Integration.Configuration;
using BOG.Integration.DTOs.CaseManagement;
using BOG.Integration.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BOG.Integration.Services.CaseManagement;

/// <summary>
/// Mock case management service for testing without actual case management system.
/// Generates mock case numbers and logs all operations.
/// </summary>
public class MockCaseManagementService : ICaseManagementService
{
    private readonly ILogger<MockCaseManagementService> _logger;
    private readonly CaseManagementSettings _settings;
    private static int _caseCounter = 1000;
    private static readonly object _lock = new();

    public MockCaseManagementService(ILogger<MockCaseManagementService> logger, IOptions<CaseManagementSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<CaseRegistrationResult> RegisterCaseAsync(CaseRegistrationData registrationData, CancellationToken cancellationToken = default)
    {
        // Validate required fields
        if (registrationData == null)
        {
            return new CaseRegistrationResult
            {
                IsSuccess = false,
                ErrorMessage = "Registration data is required"
            };
        }

        if (string.IsNullOrWhiteSpace(registrationData.Subject))
        {
            return new CaseRegistrationResult
            {
                IsSuccess = false,
                ErrorMessage = "Case subject is required"
            };
        }

        if (!registrationData.Defendants.Any())
        {
            return new CaseRegistrationResult
            {
                IsSuccess = false,
                ErrorMessage = "At least one defendant is required"
            };
        }

        // Simulate network delay
        await Task.Delay(500, cancellationToken);

        // Generate case and registration numbers
        string caseNumber;
        string registrationNumber;

        lock (_lock)
        {
            _caseCounter++;
            caseNumber = $"{_settings.MockCaseNumberPrefix}{_caseCounter}";
            registrationNumber = $"{_settings.MockRegistrationNumberPrefix}{_caseCounter}";
        }

        _logger.LogInformation("[MOCK CASE] Registered Case | CaseNumber: {CaseNumber} | Subject: {Subject} | Plaintiffs: {PlaintiffCount} | Defendants: {DefendantCount}",
            caseNumber, registrationData.Subject, registrationData.Plaintiffs.Count, registrationData.Defendants.Count);

        return new CaseRegistrationResult
        {
            IsSuccess = true,
            CaseNumber = caseNumber,
            RegistrationNumber = registrationNumber,
            RegistrationDate = DateTime.UtcNow
        };
    }

    public async Task<CaseStatusResult> GetCaseStatusAsync(string caseNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(caseNumber))
        {
            return new CaseStatusResult
            {
                IsSuccess = false,
                ErrorMessage = "Case number is required"
            };
        }

        // Simulate network delay
        await Task.Delay(300, cancellationToken);

        _logger.LogInformation("[MOCK CASE] Status Query | CaseNumber: {CaseNumber}",
            caseNumber);

        // Return mock status for any case number
        return new CaseStatusResult
        {
            IsSuccess = true,
            CaseNumber = caseNumber,
            Status = "تحت النظر",
            NextHearingDate = DateTime.UtcNow.AddDays(30),
            JudgeName = "القاضي / محمد علي"
        };
    }
}
