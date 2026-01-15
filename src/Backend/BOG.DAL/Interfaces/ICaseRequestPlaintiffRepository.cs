using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// CaseRequestPlaintiff junction table repository interface.
/// Manages the many-to-many relationship between CaseRegistrationRequest and Plaintiff.
/// </summary>
public interface ICaseRequestPlaintiffRepository : IRepository<CaseRequestPlaintiff>
{
    /// <summary>
    /// Gets all associations for a specific case request.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of associations</returns>
    Task<IEnumerable<CaseRequestPlaintiff>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all associations for a specific plaintiff.
    /// </summary>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of associations</returns>
    Task<IEnumerable<CaseRequestPlaintiff>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plaintiff is already associated with a case request.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if association exists, false otherwise</returns>
    Task<bool> ExistsAsync(int requestId, int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the association between a request and plaintiff.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The association if found, null otherwise</returns>
    Task<CaseRequestPlaintiff?> GetByRequestAndPlaintiffAsync(int requestId, int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of plaintiffs for a case request.
    /// </summary>
    /// <param name="requestId">The case request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of plaintiffs</returns>
    Task<int> GetPlaintiffCountByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);
}
