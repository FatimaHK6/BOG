namespace BOG.VM.Representative;

/// <summary>
/// Representative Attachment ViewModel for presentation layer (صورة التمثيل).
/// </summary>
public class RepresentativeAttachmentVM
{
    /// <summary>
    /// Attachment ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Representative ID.
    /// </summary>
    public int RepresentativeId { get; set; }

    /// <summary>
    /// Attachment type ID.
    /// </summary>
    public int AttachmentTypeId { get; set; }

    /// <summary>
    /// Attachment type name (English).
    /// </summary>
    public string AttachmentTypeName { get; set; } = null!;

    /// <summary>
    /// Attachment type name (Arabic).
    /// </summary>
    public string AttachmentTypeNameAr { get; set; } = null!;

    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// File content type (MIME type).
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// Download URL for the attachment.
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// Upload date.
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Description or notes.
    /// </summary>
    public string? Description { get; set; }
}
