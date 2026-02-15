namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for deficiency types (نوع النقص) - the location where a deficiency was found.
/// 6 predefined types: Case Subject, Case Claims, Case Grounds, Related Cases, Case Attachments, Additional Case Information.
/// </summary>
public class DeficiencyType : BaseEntity
{
    /// <summary>
    /// Type name in English (e.g., "CaseSubject", "CaseClaims").
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Type name in Arabic (e.g., "موضوع الدعوى", "طلبات الدعوى").
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Display order for sorting types.
    /// </summary>
    public int DisplayOrder { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Collection of deficiency descriptions for this type.
    /// </summary>
    public virtual ICollection<DeficiencyDescription> Descriptions { get; set; } = new List<DeficiencyDescription>();

    #endregion
}
