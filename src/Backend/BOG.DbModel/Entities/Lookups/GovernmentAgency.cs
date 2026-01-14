namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for government agencies (الجهة الحكومية).
/// </summary>
public class GovernmentAgency : BaseEntity
{
    /// <summary>
    /// Agency name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Agency name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Agency code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Description of the agency.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the agency is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
