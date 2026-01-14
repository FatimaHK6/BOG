using BOG.DbModel.Entities.Identity;
using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Main entity for case registration request (طلب قيد دعوى).
/// </summary>
public class CaseRegistrationRequest : BaseEntity
{
    /// <summary>
    /// Foreign key to RequestStatus.
    /// </summary>
    public int RequestStatusId { get; set; }

    /// <summary>
    /// Subject of the case (الموضوع) - max 4000 characters.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Evidence description (البيّنات) - max 4000 characters.
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// Notes (ملاحظات).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Foreign key to Court.
    /// </summary>
    public int? CourtId { get; set; }

    /// <summary>
    /// Date when the request was submitted.
    /// </summary>
    public DateTime? SubmissionDate { get; set; }

    /// <summary>
    /// Deadline for completing deficiencies (30 days from deficiency notification).
    /// </summary>
    public DateTime? CompletionDeadline { get; set; }

    /// <summary>
    /// Reason for rejection (if rejected).
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Case number assigned after registration.
    /// </summary>
    public string? CaseNumber { get; set; }

    /// <summary>
    /// Registration number.
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Date when case was registered.
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Foreign key to the user who created the request.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// Foreign key to the user who last modified the request.
    /// </summary>
    public int? LastModifiedByUserId { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property for status.
    /// </summary>
    public virtual RequestStatus Status { get; set; } = null!;

    /// <summary>
    /// Navigation property for court.
    /// </summary>
    public virtual Court? Court { get; set; }

    /// <summary>
    /// Navigation property for created by user.
    /// </summary>
    public virtual User CreatedByUser { get; set; } = null!;

    /// <summary>
    /// Navigation property for last modified by user.
    /// </summary>
    public virtual User? LastModifiedByUser { get; set; }

    /// <summary>
    /// Collection of plaintiffs (many-to-many through junction table).
    /// </summary>
    public virtual ICollection<CaseRequestPlaintiff> CaseRequestPlaintiffs { get; set; } = new List<CaseRequestPlaintiff>();

    /// <summary>
    /// Collection of defendants (many-to-many through junction table).
    /// </summary>
    public virtual ICollection<CaseRequestDefendant> CaseRequestDefendants { get; set; } = new List<CaseRequestDefendant>();

    /// <summary>
    /// Collection of claims (one-to-many).
    /// </summary>
    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    /// <summary>
    /// Collection of related cases (one-to-many).
    /// </summary>
    public virtual ICollection<RelatedCase> RelatedCases { get; set; } = new List<RelatedCase>();

    /// <summary>
    /// Collection of classifications (one-to-many).
    /// </summary>
    public virtual ICollection<RequestClassification> Classifications { get; set; } = new List<RequestClassification>();

    /// <summary>
    /// Collection of attachments (one-to-many).
    /// </summary>
    public virtual ICollection<RequestAttachment> Attachments { get; set; } = new List<RequestAttachment>();

    #endregion
}
