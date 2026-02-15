namespace BOG.VM.CaseRegistrationRequest;

/// <summary>
/// Full view model for case registration request details.
/// </summary>
public class CaseRegistrationRequestVM
{
    public int Id { get; set; }
    public int RequestStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusNameAr { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Evidence { get; set; }
    public string? Notes { get; set; }
    public int? CourtId { get; set; }
    public string? CourtName { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? CompletionDeadline { get; set; }
    public string? RejectionReason { get; set; }
    public string? CaseNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int PlaintiffsCount { get; set; }
    public int DefendantsCount { get; set; }
    public int ClaimsCount { get; set; }
    public int AttachmentsCount { get; set; }
    public List<int> ClassificationIds { get; set; } = new();
    public List<int> RelatedCaseIds { get; set; } = new();
    public string? PrimaryMobile { get; set; }
    public string? SecondaryMobile { get; set; }
    public string? Email { get; set; }
}
