namespace BOG.VM.CaseRegistration;

/// <summary>
/// View model for a case registration request (list/summary view).
/// Used in GET /api/case-requests/{id} and search results
/// </summary>
public class CaseRegistrationRequestVM
{
    /// <summary>
    /// Case registration request ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Request status ID (1=Draft, 3=New, 5=OnJudgeDesk, 6=Registered, 8=PendingCompletion, 9=UnderReview, 10=Rejected)
    /// </summary>
    public int RequestStatusId { get; set; }

    /// <summary>
    /// Request status name in English
    /// </summary>
    public string RequestStatusName { get; set; } = "";

    /// <summary>
    /// Court ID
    /// </summary>
    public int CourtId { get; set; }

    /// <summary>
    /// Court name in Arabic
    /// </summary>
    public string CourtName { get; set; } = "";

    /// <summary>
    /// Case subject/title
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Evidence description
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Date the request was submitted (moved from Draft to New)
    /// </summary>
    public DateTime? SubmissionDate { get; set; }

    /// <summary>
    /// Deadline for completing missing documents (if in PendingCompletion state)
    /// </summary>
    public DateTime? CompletionDeadline { get; set; }

    /// <summary>
    /// Reason for rejection (if rejected)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Case number assigned by external system (if registered)
    /// </summary>
    public string? CaseNumber { get; set; }

    /// <summary>
    /// Registration number in external system (if registered)
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Date the case was registered in external system
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Number of plaintiffs (calculated)
    /// </summary>
    public int PlaintiffsCount { get; set; }

    /// <summary>
    /// Number of defendants (calculated)
    /// </summary>
    public int DefendantsCount { get; set; }

    /// <summary>
    /// Number of attachments (calculated)
    /// </summary>
    public int AttachmentsCount { get; set; }

    /// <summary>
    /// List of classification IDs assigned to this request
    /// </summary>
    public List<int> ClassificationIds { get; set; } = new();

    /// <summary>
    /// Primary mobile phone number
    /// </summary>
    public string? PrimaryMobile { get; set; }

    /// <summary>
    /// Secondary mobile phone number
    /// </summary>
    public string? SecondaryMobile { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// List of related cases
    /// </summary>
    public List<RelatedCaseVM> RelatedCases { get; set; } = new();

    /// <summary>
    /// Date the request was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date the request was last modified
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
