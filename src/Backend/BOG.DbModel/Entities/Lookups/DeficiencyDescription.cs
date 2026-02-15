using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for deficiency descriptions (وصف النقص) - predefined deficiency templates.
/// Each deficiency type has multiple predefined descriptions that موظف القيد can select from.
/// </summary>
public class DeficiencyDescription : BaseEntity
{
    /// <summary>
    /// Foreign key to DeficiencyType.
    /// </summary>
    public int DeficiencyTypeId { get; set; }

    /// <summary>
    /// Description text in Arabic (e.g., "موضوع الدعوى غير واضح").
    /// </summary>
    public string DescriptionAr { get; set; } = null!;

    /// <summary>
    /// Description text in English (optional).
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Display order for sorting descriptions within the same type.
    /// </summary>
    public int DisplayOrder { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property for the deficiency type.
    /// </summary>
    public virtual DeficiencyType DeficiencyType { get; set; } = null!;

    /// <summary>
    /// Collection of request deficiencies using this description.
    /// </summary>
    public virtual ICollection<RequestDeficiency> RequestDeficiencies { get; set; } = new List<RequestDeficiency>();

    #endregion
}
