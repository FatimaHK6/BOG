using BOG.DbModel.Entities.Identity;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Court repository interface.
/// </summary>
public interface ICourtRepository : IRepository<Court>
{
    /// <summary>
    /// Gets all active courts.
    /// </summary>
    Task<IEnumerable<Court>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets courts by region.
    /// </summary>
    Task<IEnumerable<Court>> GetByRegionAsync(int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a court with its departments.
    /// </summary>
    Task<Court?> GetWithDepartmentsAsync(int courtId, CancellationToken cancellationToken = default);
}
