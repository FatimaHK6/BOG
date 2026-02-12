namespace BOG.VM.Shared;

/// <summary>
/// View Model for Classification lookup (تصنيف) - read-only representation for API responses.
/// </summary>
public class ClassificationVM
{
    /// <summary>
    /// Classification ID (معرف التصنيف).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Classification name in English.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Classification name in Arabic (اسم التصنيف).
    /// </summary>
    public string NameAr { get; set; } = "";

    /// <summary>
    /// Classification description (وصف التصنيف).
    /// </summary>
    public string? Description { get; set; }
}
