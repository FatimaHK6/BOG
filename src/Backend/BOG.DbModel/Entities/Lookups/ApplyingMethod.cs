namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup table for case registration submission methods.
/// Indicates whether the request was submitted through the court or through the online portal.
/// </summary>
public class ApplyingMethod : BaseEntity
{
    /// <summary>
    /// Method name in English (e.g., "Through Court", "Through Portal")
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Method name in Arabic (e.g., "من خلال المحكمة", "من خلال البوابة")
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Optional description explaining the submission method
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this method is currently active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;
}
