namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for nationalities (الجنسيات).
/// </summary>
public class Nationality : BaseEntity
{
    /// <summary>
    /// Nationality name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Nationality name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the nationality is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
