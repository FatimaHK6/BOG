namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup table for case types (Administrative or Disciplinary)
/// </summary>
public class CaseType : BaseEntity
{
    /// <summary>
    /// English name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Arabic name (إداري or تأديبي)
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Active status
    /// </summary>
    public bool IsActive { get; set; } = true;
}
