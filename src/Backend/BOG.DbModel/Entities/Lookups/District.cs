namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for districts (الحي) - SRS 6.3.2.
/// Districts belong to cities.
/// </summary>
public class District : BaseEntity
{
    /// <summary>
    /// District name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// District name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Foreign key to City.
    /// </summary>
    public int CityId { get; set; }

    /// <summary>
    /// Whether the district is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for city.
    /// </summary>
    public virtual City City { get; set; } = null!;
}
