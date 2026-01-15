using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// PlaintiffAttachment-specific repository interface.
/// Follows Interface Segregation Principle - defines only attachment-related operations.
/// </summary>
public interface IPlaintiffAttachmentRepository : IRepository<PlaintiffAttachment>
{
    /// <summary>
    /// Gets all attachments for a specific plaintiff.
    /// </summary>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of attachments</returns>
    Task<IEnumerable<PlaintiffAttachment>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attachments by type for a specific plaintiff.
    /// </summary>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="attachmentTypeId">The attachment type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of attachments of the specified type</returns>
    Task<IEnumerable<PlaintiffAttachment>> GetByTypeAsync(int plaintiffId, int attachmentTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plaintiff has the required attachments based on plaintiff type.
    /// </summary>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="requiredAttachmentTypeIds">List of required attachment type IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all required attachments exist, false otherwise</returns>
    Task<bool> HasRequiredAttachmentsAsync(int plaintiffId, IEnumerable<int> requiredAttachmentTypeIds, CancellationToken cancellationToken = default);
}
