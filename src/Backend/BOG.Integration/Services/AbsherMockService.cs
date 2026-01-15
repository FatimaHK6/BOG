using BOG.Integration.DTOs.Absher;
using BOG.Integration.Interfaces;
using Microsoft.Extensions.Logging;

namespace BOG.Integration.Services;

/// <summary>
/// Mock implementation of IAbsherService for development/testing.
/// </summary>
public class AbsherMockService : IAbsherService
{
    private readonly ILogger<AbsherMockService> _logger;

    private static readonly Dictionary<string, AbsherPersonData> MockData = new()
    {
        ["1000000001"] = new AbsherPersonData
        {
            FirstName = "محمد",
            FatherName = "أحمد",
            GrandfatherName = "عبدالله",
            FamilyName = "السعيد",
            FirstNameEn = "Mohammed",
            FatherNameEn = "Ahmed",
            GrandfatherNameEn = "Abdullah",
            FamilyNameEn = "AlSaeed",
            BirthDate = new DateTime(1985, 5, 15),
            Gender = "ذكر",
            NationalityId = 1,
            IdentityIssueDate = new DateTime(2020, 1, 1),
            IdentityExpiryDate = new DateTime(2030, 1, 1),
            MobileNumber = "0501234567",
            Email = "mohammed@example.com"
        },
        ["2000000001"] = new AbsherPersonData
        {
            FirstName = "فاطمة",
            FatherName = "خالد",
            GrandfatherName = "سعد",
            FamilyName = "المطيري",
            FirstNameEn = "Fatima",
            FatherNameEn = "Khaled",
            GrandfatherNameEn = "Saad",
            FamilyNameEn = "AlMutairi",
            BirthDate = new DateTime(1990, 8, 20),
            Gender = "أنثى",
            NationalityId = 1,
            IdentityIssueDate = new DateTime(2021, 3, 15),
            IdentityExpiryDate = new DateTime(2031, 3, 15),
            MobileNumber = "0559876543",
            Email = "fatima@example.com"
        }
    };

    public AbsherMockService(ILogger<AbsherMockService> logger)
    {
        _logger = logger;
    }

    public async Task<AbsherVerificationResult> VerifyIdentityAsync(string identityNumber, int identityType, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // Simulate API delay

        _logger.LogInformation("Mock Absher: Verifying identity {IdentityNumber}", identityNumber);

        // Known mock data
        if (MockData.ContainsKey(identityNumber))
        {
            return new AbsherVerificationResult { IsVerified = true };
        }

        // Any 10-digit number starting with 1 or 2 is considered valid
        if (identityNumber.Length == 10 && (identityNumber[0] == '1' || identityNumber[0] == '2'))
        {
            return new AbsherVerificationResult { IsVerified = true };
        }

        return new AbsherVerificationResult
        {
            IsVerified = false,
            ErrorCode = "ABSHER_001",
            ErrorMessage = "Identity not found",
            ErrorMessageAr = "الهوية غير موجودة"
        };
    }

    public async Task<AbsherPersonData?> GetPersonDataAsync(string identityNumber, int identityType, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Mock Absher: Getting person data for {IdentityNumber}", identityNumber);

        return MockData.TryGetValue(identityNumber, out var data) ? data : null;
    }

    public async Task<AbsherAddressData?> GetNationalAddressAsync(string identityNumber, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Mock Absher: Getting address for {IdentityNumber}", identityNumber);

        if (!MockData.ContainsKey(identityNumber))
            return null;

        return new AbsherAddressData
        {
            ResidenceBuildingNumber = "1234",
            ResidenceStreetName = "شارع الملك فهد",
            ResidenceDistrict = "العليا",
            ResidenceCity = "الرياض",
            ResidencePostalCode = "12345",
            ResidenceAdditionalNumber = "6789",
            WorkBuildingNumber = "5678",
            WorkStreetName = "شارع التحلية",
            WorkDistrict = "السليمانية",
            WorkCity = "الرياض",
            WorkPostalCode = "12346",
            WorkAdditionalNumber = "1234"
        };
    }
}
