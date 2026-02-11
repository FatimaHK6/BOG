namespace BOG.VM.Plaintiff;

/// <summary>
/// Plaintiff Attachment ViewModel.
/// </summary>
public class PlaintiffAttachmentVM
{
    /// <summary>
    /// Attachment ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Plaintiff ID.
    /// </summary>
    public int PlaintiffId { get; set; }

    /// <summary>
    /// Attachment type ID.
    /// </summary>
    public int AttachmentTypeId { get; set; }

    /// <summary>
    /// Attachment type name (Arabic).
    /// </summary>
    public string AttachmentTypeNameAr { get; set; } = null!;

    /// <summary>
    /// Attachment type name (English).
    /// </summary>
    public string AttachmentTypeName { get; set; } = null!;

    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// File content type (MIME type).
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// File size display (KB/MB).
    /// </summary>
    public string FileSizeDisplay
    {
        get
        {
            if (FileSizeBytes < 1024)
                return $"{FileSizeBytes} B";
            if (FileSizeBytes < 1024 * 1024)
                return $"{FileSizeBytes / 1024.0:F1} KB";
            return $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
        }
    }

    /// <summary>
    /// Description or notes.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Upload date.
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Download URL (to be set by controller).
    /// </summary>
    public string? DownloadUrl { get; set; }
}
