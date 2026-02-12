namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Tracks workflow history for case registration requests.
/// Records all status transitions and actions taken.
/// </summary>
public class CaseRequestWorkflow : BaseEntity
{
    /// <summary>
    /// Case registration request ID
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Previous status ID (before transition)
    /// </summary>
    public int PreviousStatusId { get; set; }

    /// <summary>
    /// New status ID (after transition)
    /// </summary>
    public int NewStatusId { get; set; }

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// User's full name (from User.FullName field)
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Notes/remarks for the action
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Action timestamp (should be same as CreatedDate)
    /// </summary>
    public DateTime ActionDate { get; set; }

    // Navigation
    public virtual CaseRegistrationRequest? CaseRegistrationRequest { get; set; }
}
