namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for case classifications (تصنيف الدعوى).
/// Types: Civil Case, Commercial Case, Labor Case, Family Case, etc.
/// </summary>
public class Classification : BaseEntity
{
    /// <summary>
    /// Classification name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Classification name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Description of the classification.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Hierarchical Level 1 (Main category). Example: "عقود"
    /// </summary>
    public string? Level1 { get; set; }

    /// <summary>
    /// Hierarchical Level 2 (Subcategory). Example: "عقود مدنية"
    /// </summary>
    public string? Level2 { get; set; }

    /// <summary>
    /// Hierarchical Level 3 (Specific type). Example: "عقود البيع"
    /// </summary>
    public string? Level3 { get; set; }

    /// <summary>
    /// Hierarchical Level 4 (Detailed type). Example: "عقد بيع عقار"
    /// </summary>
    public string? Level4 { get; set; }

    /// <summary>
    /// Whether the classification is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
