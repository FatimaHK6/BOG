namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for administrative regions (المنطقة الإدارية).
/// Saudi Arabia has 13 administrative regions.
/// </summary>
public class Region : BaseEntity
{
    /// <summary>
    /// Region name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Region name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Region code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Whether the region is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for cities in this region.
    /// </summary>
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
