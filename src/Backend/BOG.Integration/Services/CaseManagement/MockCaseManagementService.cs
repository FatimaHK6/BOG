using System.Globalization;
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

        // Generate case number using Hijri calendar: {counter}/{courtArabicName}/{hijri_year}
        string caseNumber;
        string registrationNumber;

        // Get Hijri date using Islamic (Hijri) calendar
        // Note: Using HijriCalendar for Hijri dates. In production, integrate with official Umm al-Qura calendar service
        var hijriCalendar = new System.Globalization.HijriCalendar();
        var now = DateTime.Now;
        int hijriYear = hijriCalendar.GetYear(now);
        int hijriMonth = hijriCalendar.GetMonth(now);
        int hijriDay = hijriCalendar.GetDayOfMonth(now);

        // Get court name (in production, fetch from database)
        int courtId = registrationData.CourtId ?? 1;
        string courtName = registrationData.CourtName ?? GetCourtNameById(courtId);

        // Get counter for this court and year
        int counter;
        lock (_lock)
        {
            // In production, this should query database:
            // SELECT ISNULL(MAX(CaseCounter), 0) + 1 FROM CaseRegistrationRequests
            // WHERE CourtId = @CourtId AND HijriYear = @HijriYear
            _caseCounter++;
            counter = _caseCounter % 1000; // Mock: reset every 1000 for demo
        }

        // Format: {counter}/{courtArabicName}/{hijri_year}
        caseNumber = $"{counter}/{courtName}/{hijriYear}";

        // Registration number format: {court_id}-{year}-{counter}
        registrationNumber = $"{courtId}-{hijriYear}-{counter}";

        _logger.LogInformation("[MOCK CASE] Generated case number: {CaseNumber} (Hijri: {Day}/{Month}/{Year})",
            caseNumber, hijriDay, hijriMonth, hijriYear);

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

    private string GetCourtNameById(int courtId)
    {
        // Mock court names - in production, fetch from database
        return courtId switch
        {
            1 => "المحكمة الإدارية بالرياض",
            2 => "المحكمة الإدارية بجدة",
            3 => "المحكمة الإدارية بمكة المكرمة",
            4 => "المحكمة الإدارية بالدمام",
            5 => "المحكمة الإدارية بالمدينة المنورة",
            _ => $"المحكمة الإدارية {courtId}"
        };
    }
}
