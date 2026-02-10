using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Repository interface for RequestAttachment entity.
/// Handles request-level attachments (PDFs, documents, etc.).
/// Follows Repository Pattern.
/// </summary>
public interface IRequestAttachmentRepository : IRepository<RequestAttachment>
{
    /// <summary>
    /// Gets all attachments for a specific case registration request.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerable of attachments for the request</returns>
    Task<IEnumerable<RequestAttachment>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);
}
