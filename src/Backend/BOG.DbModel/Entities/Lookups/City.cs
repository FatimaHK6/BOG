namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for cities (المدينة).
/// </summary>
public class City : BaseEntity
{
    /// <summary>
    /// City name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// City name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// City code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Foreign key to Region.
    /// </summary>
    public int RegionId { get; set; }

    /// <summary>
    /// Whether the city is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for the region.
    /// </summary>
    public virtual Region Region { get; set; } = null!;
}
