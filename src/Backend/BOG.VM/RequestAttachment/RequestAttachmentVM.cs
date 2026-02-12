namespace BOG.VM.RequestAttachment;

/// <summary>
/// View model for a request attachment.
/// Used in GET /api/case-requests/{requestId}/attachments
/// </summary>
public class RequestAttachmentVM
{
    /// <summary>
    /// Attachment ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Case registration request ID
    /// </summary>
    public int RequestId { get; set; }

    /// <summary>
    /// Attachment type ID
    /// </summary>
    public int AttachmentTypeId { get; set; }

    /// <summary>
    /// Attachment type name in Arabic
    /// </summary>
    public string AttachmentTypeName { get; set; } = "";

    /// <summary>
    /// File name with extension
    /// </summary>
    public string FileName { get; set; } = "";

    /// <summary>
    /// File size in bytes
    /// </summary>
    public int FileSize { get; set; }

    /// <summary>
    /// File size in kilobytes (formatted for display)
    /// </summary>
    public double FileSizeKb { get; set; }

    /// <summary>
    /// Is mandatory attachment type
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Date the attachment was uploaded
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date the attachment was last modified
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Download URL for the attachment
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// Attachment description/notes (optional)
    /// </summary>
    public string? Description { get; set; }
}
