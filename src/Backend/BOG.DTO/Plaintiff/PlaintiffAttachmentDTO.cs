using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Plaintiff;

/// <summary>
/// DTO for creating a plaintiff attachment.
/// </summary>
public class PlaintiffAttachmentCreateDTO
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
    /// File content as Base64 string.
    /// </summary>
    [Required(ErrorMessage = "محتوى الملف مطلوب")]
    public string FileContent { get; set; } = null!;

    /// <summary>
    /// File content type (MIME type). Must be application/pdf.
    /// </summary>
    [Required(ErrorMessage = "نوع المحتوى مطلوب")]
    [RegularExpression(@"^application/pdf$", ErrorMessage = "يجب أن يكون الملف بصيغة PDF فقط")]
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// Description or notes about the attachment.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// DTO for deleting a plaintiff attachment.
/// </summary>
public class PlaintiffAttachmentDeleteDTO
{
    /// <summary>
    /// Attachment ID to delete.
    /// </summary>
    [Required]
    public int AttachmentId { get; set; }
}
