using BOG.DbModel.Entities.Identity;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Role repository interface.
/// Follows Interface Segregation Principle - defines only role-related operations.
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<IEnumerable<Role>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets roles assigned to a user.
    /// </summary>
    Task<IEnumerable<Role>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
}
