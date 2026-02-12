namespace BOG.Integration.DTOs.CaseManagement;

/// <summary>
/// Result of case registration in external system.
/// </summary>
public class CaseRegistrationResult
{
    /// <summary>
    /// Whether registration was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Case number assigned by the court system.
    /// </summary>
    public string? CaseNumber { get; set; }

    /// <summary>
    /// Registration number assigned by the system.
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Date/time of registration.
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Error message if registration failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code from external system.
    /// </summary>
    public string? ErrorCode { get; set; }
}
