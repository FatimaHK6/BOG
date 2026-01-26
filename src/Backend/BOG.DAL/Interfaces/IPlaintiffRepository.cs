using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Plaintiff-specific repository interface.
/// Follows Interface Segregation Principle - defines only plaintiff-related operations.
/// </summary>
public interface IPlaintiffRepository : IRepository<Plaintiff>
{
    /// <summary>
    /// Gets all plaintiffs for a specific case request.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of plaintiffs</returns>
    Task<IEnumerable<Plaintiff>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plaintiff by identity number within a specific request.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="identityNumber">The identity number</param>
    /// <param name="plaintiffTypeId">The plaintiff type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The plaintiff if found, null otherwise</returns>
    Task<Plaintiff?> GetByIdentityAsync(int requestId, string identityNumber, int plaintiffTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plaintiff with the given identity exists in a request.
    /// Used for duplicate checking (ERR011).
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="identityNumber">The identity number</param>
    /// <param name="plaintiffTypeId">The plaintiff type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if plaintiff exists, false otherwise</returns>
    Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int plaintiffTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plaintiff with the given document number exists in a request.
    /// Used for duplicate checking (ERR011) for Type 2 - Individual without ID.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="documentNumber">The document number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if plaintiff exists, false otherwise</returns>
    Task<bool> ExistsByDocumentNumberAsync(int requestId, string documentNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plaintiff with all related details (representatives, attachments, addresses).
    /// </summary>
    /// <param name="id">The plaintiff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The plaintiff with details if found, null otherwise</returns>
    Task<Plaintiff?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the applicant (مقدم الطلب) for a specific case request.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The applicant plaintiff if set, null otherwise</returns>
    Task<Plaintiff?> GetApplicantByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);
}
