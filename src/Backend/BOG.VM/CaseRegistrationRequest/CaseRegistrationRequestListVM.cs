namespace BOG.VM.CaseRegistrationRequest;

/// <summary>
/// View model for case registration request list.
/// </summary>
public class CaseRegistrationRequestListVM
{
    public int Id { get; set; }
    public string? CaseNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public int RequestStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusNameAr { get; set; } = string.Empty;
    public string? CourtName { get; set; }
    public string? CourtNameAr { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int PlaintiffsCount { get; set; }
    public int DefendantsCount { get; set; }

    // Additional fields for frontend
    public bool IsDraft { get; set; }
    public int? CaseTypeId { get; set; }
    public string? SubjectPreview { get; set; }
    public string? PlaintiffName { get; set; }
    public string? DefendantName { get; set; }
}
