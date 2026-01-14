using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for case registration request attachments (مرفقات الطلب).
/// </summary>
public class RequestAttachment : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Foreign key to AttachmentType.
    /// </summary>
    public int AttachmentTypeId { get; set; }

    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Stored file name/path.
    /// </summary>
    public string StoredFileName { get; set; } = null!;

    /// <summary>
    /// File content type (MIME type).
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Description or notes about the attachment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Upload date.
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Whether the attachment is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;

    /// <summary>
    /// Navigation property for attachment type.
    /// </summary>
    public virtual AttachmentType AttachmentType { get; set; } = null!;
}
