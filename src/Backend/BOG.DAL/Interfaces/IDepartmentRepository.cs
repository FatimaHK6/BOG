using BOG.DbModel.Entities.Identity;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Department repository interface.
/// </summary>
public interface IDepartmentRepository : IRepository<Department>
{
    /// <summary>
    /// Gets all active departments.
    /// </summary>
    Task<IEnumerable<Department>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets departments by court.
    /// </summary>
    Task<IEnumerable<Department>> GetByCourtAsync(int courtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets departments assigned to a user.
    /// </summary>
    Task<IEnumerable<Department>> GetUserDepartmentsAsync(int userId, CancellationToken cancellationToken = default);
}
