namespace BOG.VM.Defendant;

/// <summary>
/// View model for a defendant in list view (compact view).
/// Used in GET /api/case-requests/{requestId}/defendants
/// </summary>
public class DefendantListVM
{
    /// <summary>
    /// Defendant ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Full name of the defendant
    /// </summary>
    public string FullName { get; set; } = "";

    /// <summary>
    /// Defendant type name in Arabic
    /// </summary>
    public string DefendantTypeName { get; set; } = "";

    /// <summary>
    /// Identity number (if available, partially masked)
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Identity type name
    /// </summary>
    public string? IdentityTypeName { get; set; }

    /// <summary>
    /// City/location (extracted from address)
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Is the defendant active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date the defendant was created
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
