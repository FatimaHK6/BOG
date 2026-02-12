namespace BOG.Integration.DTOs.CaseManagement;

/// <summary>
/// Result of querying case status from external system.
/// </summary>
public class CaseStatusResult
{
    /// <summary>
    /// Whether query was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Case number.
    /// </summary>
    public string? CaseNumber { get; set; }

    /// <summary>
    /// Current status of the case.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Next scheduled hearing date.
    /// </summary>
    public DateTime? NextHearingDate { get; set; }

    /// <summary>
    /// Judge assigned to the case.
    /// </summary>
    public string? JudgeName { get; set; }

    /// <summary>
    /// Error message if query failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
