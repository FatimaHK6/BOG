namespace BOG.Integration.DTOs.Absher;

/// <summary>
/// Result of Absher identity verification.
/// </summary>
public class AbsherVerificationResult
{
    public bool IsVerified { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorMessageAr { get; set; }
}

/// <summary>
/// Personal data from Absher.
/// </summary>
public class AbsherPersonData
{
    public string FirstName { get; set; } = null!;
    public string FatherName { get; set; } = null!;
    public string GrandfatherName { get; set; } = null!;
    public string FamilyName { get; set; } = null!;
    public string? FirstNameEn { get; set; }
    public string? FatherNameEn { get; set; }
    public string? GrandfatherNameEn { get; set; }
    public string? FamilyNameEn { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = null!;
    public int NationalityId { get; set; }
    public DateTime? IdentityIssueDate { get; set; }
    public DateTime? IdentityExpiryDate { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// Address data from Absher.
/// </summary>
public class AbsherAddressData
{
    // Residence Address
    public string? ResidenceBuildingNumber { get; set; }
    public string? ResidenceStreetName { get; set; }
    public string? ResidenceDistrict { get; set; }
    public string? ResidenceCity { get; set; }
    public string? ResidencePostalCode { get; set; }
    public string? ResidenceAdditionalNumber { get; set; }

    // Work Address
    public string? WorkBuildingNumber { get; set; }
    public string? WorkStreetName { get; set; }
    public string? WorkDistrict { get; set; }
    public string? WorkCity { get; set; }
    public string? WorkPostalCode { get; set; }
    public string? WorkAdditionalNumber { get; set; }
}
