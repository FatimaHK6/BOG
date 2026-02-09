namespace BOG.VM.Defendant;

/// <summary>
/// Defendant ViewModel for list/summary presentation.
/// </summary>
public class DefendantListVM
{
    /// <summary>
    /// Defendant ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Defendant type ID.
    /// </summary>
    public int DefendantTypeId { get; set; }

    /// <summary>
    /// Defendant type name (Arabic).
    /// </summary>
    public string DefendantTypeNameAr { get; set; } = null!;

    /// <summary>
    /// Display name (full name or company name).
    /// </summary>
    public string DisplayName { get; set; } = null!;
}
