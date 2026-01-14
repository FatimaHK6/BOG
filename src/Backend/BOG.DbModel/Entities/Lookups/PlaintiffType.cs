namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for plaintiff types (نوع المدعي).
/// Types: Individual, Company, Government Agency, Society, Waqf, Minor/Incapacitated, Heir, Bankrupt Estate
/// </summary>
public class PlaintiffType : BaseEntity
{
    /// <summary>
    /// Type name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Type name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Description of the type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the type is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
