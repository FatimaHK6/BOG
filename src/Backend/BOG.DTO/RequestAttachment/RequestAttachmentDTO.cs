using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.RequestAttachment;

/// <summary>
/// Data transfer object for uploading an attachment to a case registration request.
/// Used in POST /api/case-requests/{requestId}/attachments
/// Enforces BR04: PDF only, max 4MB
/// </summary>
public class RequestAttachmentDTO
{
    /// <summary>
    /// Attachment type ID (required)
    /// Specifies what type of document is being uploaded (ID, Address proof, Commercial register, etc.)
    /// </summary>
    [Required(ErrorMessage = "Attachment type is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid attachment type")]
    public int AttachmentTypeId { get; set; }

    /// <summary>
    /// File name with extension (required)
    /// </summary>
    [Required(ErrorMessage = "File name is required")]
    [StringLength(255, MinimumLength = 5, ErrorMessage = "File name must be between 5 and 255 characters")]
    public string FileName { get; set; } = null!;

    /// <summary>
    /// File content as Base64 string (required)
    /// Alternative: Can be received as multipart/form-data file
    /// Must be PDF format (BR04)
    /// Maximum size: 4MB (BR04)
    /// </summary>
    [Required(ErrorMessage = "File content is required")]
    public byte[] FileContent { get; set; } = null!;

    /// <summary>
    /// Content type of the file (validated in BL: must be application/pdf per BR04)
    /// </summary>
    [Required(ErrorMessage = "Content type is required")]
    public string ContentType { get; set; } = "application/pdf";

    /// <summary>
    /// Additional description or notes about the attachment (optional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
