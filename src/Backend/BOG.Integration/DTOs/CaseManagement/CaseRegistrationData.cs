namespace BOG.Integration.DTOs.CaseManagement;

/// <summary>
/// Data for registering a case in external case management system.
/// </summary>
public class CaseRegistrationData
{
    /// <summary>
    /// Request ID in BOG system.
    /// </summary>
    public int RequestId { get; set; }

    /// <summary>
    /// Court ID where the case will be registered.
    /// </summary>
    public int? CourtId { get; set; }

    /// <summary>
    /// Court name in Arabic (for case number generation).
    /// </summary>
    public string? CourtName { get; set; }

    /// <summary>
    /// Subject of the case.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Evidence description.
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Case type/classification.
    /// </summary>
    public string? CaseType { get; set; }

    /// <summary>
    /// List of plaintiffs.
    /// </summary>
    public List<CasePartyData> Plaintiffs { get; set; } = new();

    /// <summary>
    /// List of defendants.
    /// </summary>
    public List<CasePartyData> Defendants { get; set; } = new();

    /// <summary>
    /// Attached documents (file names or URLs).
    /// </summary>
    public List<string> AttachmentReferences { get; set; } = new();

    /// <summary>
    /// Additional notes or comments.
    /// </summary>
    public string? AdditionalNotes { get; set; }
}
