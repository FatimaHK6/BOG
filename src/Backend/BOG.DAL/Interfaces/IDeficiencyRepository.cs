using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Repository interface for RequestDeficiency entity.
/// Handles data access for deficiencies (نواقص الدعوى) of case registration requests.
/// </summary>
public interface IDeficiencyRepository : IRepository<RequestDeficiency>
{
    /// <summary>
    /// Gets all deficiencies for a specific request.
    /// Filters soft-deleted records.
    /// Orders by display order.
    /// </summary>
    Task<IEnumerable<RequestDeficiency>> GetByRequestIdAsync(int requestId, CancellationToken ct = default);

    /// <summary>
    /// Soft deletes all deficiencies for a specific request.
    /// Used when replacing deficiencies in batch update.
    /// </summary>
    Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default);
}
