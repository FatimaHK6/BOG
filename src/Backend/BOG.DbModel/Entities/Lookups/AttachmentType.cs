namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for attachment types (نوع المرفق).
/// Types: Identity Copy, Power of Attorney, Commercial Registration, License, Deed, etc.
/// </summary>
public class AttachmentType : BaseEntity
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
    /// Whether this attachment type is mandatory.
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Maximum file size in bytes (default 4MB = 4194304).
    /// </summary>
    public int MaxFileSizeBytes { get; set; } = 4194304;

    /// <summary>
    /// Allowed file extensions (e.g., ".pdf").
    /// </summary>
    public string AllowedExtensions { get; set; } = ".pdf";

    /// <summary>
    /// Whether the type is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
