namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for representative types (نوع الممثل).
/// Types: Agent, Guardian, Custodian, Executor, Heir Representative, Company Representative, etc.
/// </summary>
public class RepresentativeType : BaseEntity
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
