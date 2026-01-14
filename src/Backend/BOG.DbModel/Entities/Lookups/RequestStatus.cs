namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for case registration request statuses (حالة الطلب).
/// Statuses: Draft, New, Under Review, Registered, Rejected, Pending Completion, On Judge Desk, etc.
/// </summary>
public class RequestStatus : BaseEntity
{
    /// <summary>
    /// Status name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Status name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Description of the status.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the status is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
