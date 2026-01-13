using BOG.DbModel.Entities;

namespace BOG.DAL.Interfaces;

/// <summary>
/// User-specific repository interface.
/// Follows Interface Segregation Principle - defines only user-related operations.
/// Follows Dependency Inversion Principle - allows injection of different implementations.
/// Inherits from generic IRepository<User> for standard CRUD operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active users</returns>
    Task<IEnumerable<User>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given email exists.
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
