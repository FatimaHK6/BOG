namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for defendant types (نوع المدعى عليه).
/// Types: Individual, Company, Government Agency, Society, Waqf, Unknown
/// </summary>
public class DefendantType : BaseEntity
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
