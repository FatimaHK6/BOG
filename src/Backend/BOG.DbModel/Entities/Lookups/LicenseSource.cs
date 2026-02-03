namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for license sources (مصدر الترخيص) - SRS 6.3.4.
/// </summary>
public class LicenseSource : BaseEntity
{
    /// <summary>
    /// License source name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// License source name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// License source code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Whether the license source is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
