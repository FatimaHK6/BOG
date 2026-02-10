using BOG.VM.RequestAttachment;

namespace BOG.BL.Interfaces.CaseRegistration;

/// <summary>
/// Business logic service for case registration request attachments.
/// Handles attachment upload, validation, retrieval, and deletion.
/// Enforces business rule BR04: PDF only, max 4MB per file.
/// </summary>
public interface IRequestAttachmentBL
{
    /// <summary>
    /// Adds a new attachment to a case registration request.
    /// Validates:
    /// - BR04: File must be PDF format
    /// - BR04: File size must not exceed 4MB
    /// - Request must exist and not be deleted
    /// - Attachment type must be valid
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="attachmentData">Attachment details (file, type, description)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created request attachment view model</returns>
    Task<RequestAttachmentVM> AddAttachmentAsync(int requestId, object attachmentData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all attachments for a case registration request.
    /// Filters out deleted attachments.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of request attachment view models</returns>
    Task<IEnumerable<RequestAttachmentVM>> GetAttachmentsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mandatory attachments for a case registration request.
    /// Only returns attachments whose type is marked as mandatory.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of mandatory request attachment view models</returns>
    Task<IEnumerable<RequestAttachmentVM>> GetMandatoryAttachmentsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific attachment by ID.
    /// </summary>
    /// <param name="attachmentId">Attachment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Request attachment view model or null if not found</returns>
    Task<RequestAttachmentVM?> GetAttachmentByIdAsync(int attachmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes an attachment.
    /// Only allowed in Draft and PendingCompletion states.
    /// </summary>
    /// <param name="attachmentId">Attachment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing async operation</returns>
    Task DeleteAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mandatory attachment types that are still missing for the request.
    /// Used during submission validation (ERR003).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of missing mandatory attachment types</returns>
    Task<IEnumerable<object>> GetMissingMandatoryAttachmentsAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads attachment file content.
    /// </summary>
    /// <param name="attachmentId">Attachment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (fileName, fileContent in bytes)</returns>
    Task<(string FileName, byte[] FileContent)> DownloadAttachmentAsync(int attachmentId, CancellationToken cancellationToken = default);
}
