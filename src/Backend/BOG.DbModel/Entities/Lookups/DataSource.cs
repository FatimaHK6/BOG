namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for data source (مصدر البيانات).
/// Types: FromAbsher (من أبشر), FromUser (من المستخدم)
/// </summary>
public class DataSource : BaseEntity
{
    /// <summary>
    /// Source name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Source name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Description of the source.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the source is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
