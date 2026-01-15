using BOG.DTO.Plaintiff;
using BOG.VM.Plaintiff;

namespace BOG.BL.Interfaces;

/// <summary>
/// Plaintiff Attachment business logic interface.
/// Handles attachment management operations.
/// </summary>
public interface IPlaintiffAttachmentBL
{
    /// <summary>
    /// Gets all attachments for a plaintiff.
    /// </summary>
    Task<IEnumerable<PlaintiffAttachmentVM>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an attachment by ID.
    /// </summary>
    Task<PlaintiffAttachmentVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new attachment for a plaintiff.
    /// Validates PDF only, max 4MB (BR04).
    /// </summary>
    Task<PlaintiffAttachmentVM> CreateAsync(int plaintiffId, PlaintiffAttachmentCreateDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an attachment (soft delete).
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the attachment file content for download.
    /// </summary>
    Task<(byte[] Content, string FileName, string ContentType)?> GetFileAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if plaintiff has required attachments based on type.
    /// </summary>
    Task<bool> HasRequiredAttachmentsAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets required attachment types for a plaintiff type.
    /// </summary>
    IEnumerable<int> GetRequiredAttachmentTypes(int plaintiffTypeId);
}
