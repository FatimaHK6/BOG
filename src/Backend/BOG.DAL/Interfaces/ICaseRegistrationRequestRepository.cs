using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Repository interface for CaseRegistrationRequest entity.
/// Extends generic IRepository with request-specific queries including state management.
/// Follows Repository Pattern and Dependency Inversion Principle.
/// </summary>
public interface ICaseRegistrationRequestRepository : IRepository<CaseRegistrationRequest>
{
    /// <summary>
    /// Gets a case registration request with all related navigation properties
    /// (plaintiffs, defendants, claims, attachments, etc.).
    /// </summary>
    /// <param name="requestId">The request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Request with loaded navigation properties, or null if not found</returns>
    Task<CaseRegistrationRequest?> GetWithDetailsAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all requests with a specific status.
    /// </summary>
    /// <param name="statusId">The request status ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerable of requests with the specified status</returns>
    Task<IEnumerable<CaseRegistrationRequest>> GetByStatusAsync(int statusId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all requests in PendingCompletion status where completion deadline has expired.
    /// Used for BR05 (auto-rejection after 30 days).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerable of expired requests in PendingCompletion status</returns>
    Task<IEnumerable<CaseRegistrationRequest>> GetPendingCompletionExpiredAsync(CancellationToken cancellationToken = default);
}
