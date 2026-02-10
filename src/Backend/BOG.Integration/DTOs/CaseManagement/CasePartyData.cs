namespace BOG.Integration.DTOs.CaseManagement;

/// <summary>
/// Data for a case party (plaintiff or defendant).
/// </summary>
public class CasePartyData
{
    /// <summary>
    /// Full name of the party.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Identity number (ID or passport).
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Type of identity (e.g., "Saudi ID", "Passport").
    /// </summary>
    public string? IdentityType { get; set; }

    /// <summary>
    /// Address of the party.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Representative information if applicable.
    /// </summary>
    public string? RepresentativeName { get; set; }

    /// <summary>
    /// Representative contact.
    /// </summary>
    public string? RepresentativeContact { get; set; }
}
