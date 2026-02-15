namespace BOG.VM.Lookups;

/// <summary>
/// View Model for DeficiencyDescription lookup entity (وصف النقص).
/// Used for API responses showing predefined deficiency templates.
/// </summary>
public class DeficiencyDescriptionVM
{
    /// <summary>
    /// Unique identifier for the deficiency description.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to DeficiencyType.
    /// </summary>
    public int DeficiencyTypeId { get; set; }

    /// <summary>
    /// Description text in Arabic (e.g., "موضوع الدعوى غير واضح").
    /// </summary>
    public string DescriptionAr { get; set; } = "";

    /// <summary>
    /// Description text in English (optional).
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Display order for sorting descriptions within the same type.
    /// </summary>
    public int DisplayOrder { get; set; }
}
