using BOG.VM.AdditionalInfo;
using BOG.VM.Defendant;
using BOG.VM.RequestAttachment;

namespace BOG.VM.CaseRegistration;

/// <summary>
/// View model for a case registration request with all related details.
/// Used in GET /api/case-requests/{id}/details
/// Includes plaintiffs, defendants, attachments, classifications, etc.
/// </summary>
public class CaseRegistrationRequestDetailsVM
{
    /// <summary>
    /// Case registration request ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Request status ID
    /// </summary>
    public int RequestStatusId { get; set; }

    /// <summary>
    /// Request status name
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
    /// Case subject
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Evidence description
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Submission date
    /// </summary>
    public DateTime? SubmissionDate { get; set; }

    /// <summary>
    /// Completion deadline
    /// </summary>
    public DateTime? CompletionDeadline { get; set; }

    /// <summary>
    /// Rejection reason
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Case number (if registered)
    /// </summary>
    public string? CaseNumber { get; set; }

    /// <summary>
    /// Registration number
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Registration date
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// List of plaintiffs
    /// </summary>
    public List<PlaintiffVM> Plaintiffs { get; set; } = new();

    /// <summary>
    /// List of defendants
    /// </summary>
    public List<DefendantListVM> Defendants { get; set; } = new();

    /// <summary>
    /// List of case classifications
    /// </summary>
    public List<CaseClassificationVM> Classifications { get; set; } = new();

    /// <summary>
    /// List of attachments
    /// </summary>
    public List<RequestAttachmentVM> Attachments { get; set; } = new();

    /// <summary>
    /// Additional information
    /// </summary>
    public AdditionalInfoVM? AdditionalInfo { get; set; }

    /// <summary>
    /// Case Type ID (إداري or تأديبي)
    /// </summary>
    public int CaseTypeId { get; set; }

    /// <summary>
    /// Case Type Name in Arabic (إداري or تأديبي)
    /// </summary>
    public string? CaseTypeName { get; set; }

    /// <summary>
    /// Submission method ID
    /// </summary>
    public int? ApplyingMethodId { get; set; }

    /// <summary>
    /// Submission method name in Arabic
    /// </summary>
    public string? ApplyingMethodNameAr { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date modified
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// View model for a plaintiff in request details
/// </summary>
public class PlaintiffVM
{
    /// <summary>
    /// Plaintiff ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = "";

    /// <summary>
    /// Identity number (if available)
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Is the applicant (principal party)
    /// </summary>
    public bool IsApplicant { get; set; }

    /// <summary>
    /// Mobile number (if available)
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Email address (if available)
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// View model for a case classification
/// </summary>
public class CaseClassificationVM
{
    /// <summary>
    /// Classification ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Classification name in Arabic
    /// </summary>
    public string NameAr { get; set; } = "";

    /// <summary>
    /// Classification name in English
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Classification code
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// First level of classification hierarchy (e.g., "عقود")
    /// </summary>
    public string Level1 { get; set; } = "";

    /// <summary>
    /// Second level of classification hierarchy (e.g., "عقود مدنية")
    /// </summary>
    public string Level2 { get; set; } = "";

    /// <summary>
    /// Third level of classification hierarchy (e.g., "عقود البيع")
    /// </summary>
    public string Level3 { get; set; } = "";

    /// <summary>
    /// Fourth level of classification hierarchy (e.g., "عقد بيع عقار")
    /// </summary>
    public string Level4 { get; set; } = "";
}
