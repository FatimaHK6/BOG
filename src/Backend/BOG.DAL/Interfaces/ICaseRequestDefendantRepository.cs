using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// CaseRequestDefendant junction table repository interface.
/// </summary>
public interface ICaseRequestDefendantRepository : IRepository<CaseRequestDefendant>
{
    /// <summary>
    /// Gets all associations for a specific case request.
    /// </summary>
    Task<IEnumerable<CaseRequestDefendant>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a defendant is already associated with a case request.
    /// </summary>
    Task<bool> ExistsAsync(int requestId, int defendantId, CancellationToken cancellationToken = default);
}
