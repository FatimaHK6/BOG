namespace BOG.VM.Lookups;

/// <summary>
/// View Model for DeficiencyType lookup entity (نوع النقص).
/// Used for API responses showing deficiency location types.
/// </summary>
public class DeficiencyTypeVM
{
    /// <summary>
    /// Unique identifier for the deficiency type.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type name in English (e.g., "CaseSubject", "CaseClaims").
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Type name in Arabic (e.g., "موضوع الدعوى", "طلبات الدعوى").
    /// </summary>
    public string NameAr { get; set; } = "";

    /// <summary>
    /// Display order for sorting types in the UI.
    /// </summary>
    public int DisplayOrder { get; set; }
}
