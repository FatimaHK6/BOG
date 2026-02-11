using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Representative;

/// <summary>
/// DTO for representative attachment (صورة التمثيل).
/// </summary>
public class RepresentativeAttachmentDTO
{
    /// <summary>
    /// Attachment type ID.
    /// </summary>
    [Required(ErrorMessage = "نوع المرفق مطلوب")]
    public int AttachmentTypeId { get; set; }

    /// <summary>
    /// Original file name.
    /// </summary>
    [Required(ErrorMessage = "اسم الملف مطلوب")]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    /// <summary>
    /// File content type (MIME type).
    /// </summary>
    [Required(ErrorMessage = "نوع المحتوى مطلوب")]
    [StringLength(100)]
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Base64 encoded file content.
    /// </summary>
    public string? FileContent { get; set; }

    /// <summary>
    /// Description or notes.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
}
