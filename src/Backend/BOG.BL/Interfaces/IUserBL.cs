using BOG.DTO.User;
using BOG.VM.User;

namespace BOG.BL.Interfaces;

/// <summary>
/// User business logic interface.
/// Follows Interface Segregation Principle - defines only user-related business operations.
/// Follows Dependency Inversion Principle - allows injection of different implementations.
/// </summary>
public interface IUserBL
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="createDto">The user data to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user ViewModel</returns>
    Task<UserVM> CreateUserAsync(UserCreateDTO createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user ViewModel if found, null otherwise</returns>
    Task<UserVM?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user ViewModel if found, null otherwise</returns>
    Task<UserVM?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user ViewModels</returns>
    Task<IEnumerable<UserVM>> GetAllActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="userId">The user ID to update</param>
    /// <param name="updateDto">The updated user data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user ViewModel</returns>
    Task<UserVM> UpdateUserAsync(int userId, UserUpdateDTO updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user (soft delete).
    /// </summary>
    /// <param name="userId">The user ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates or deactivates a user account.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="isActive">Whether the user should be active</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user ViewModel</returns>
    Task<UserVM> SetUserActiveStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default);
}
