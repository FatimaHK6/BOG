using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Repository interface for CaseRegistrationRequest entity.
/// </summary>
public interface ICaseRegistrationRequestRepository : IRepository<CaseRegistrationRequest>
{
    /// <summary>
    /// Gets all requests for a specific user with related data.
    /// </summary>
    Task<IEnumerable<CaseRegistrationRequest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a request with all related data.
    /// </summary>
    Task<CaseRegistrationRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a request with all related data for update operations (entity is tracked).
    /// Use this when the entity will be modified to ensure EF Core properly tracks changes.
    /// </summary>
    Task<CaseRegistrationRequest?> GetWithDetailsForUpdateAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all requests in PendingCompletion status that have expired.
    /// </summary>
    Task<IEnumerable<CaseRegistrationRequest>> GetPendingCompletionExpiredAsync(CancellationToken cancellationToken = default);
}
