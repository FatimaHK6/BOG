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
}
