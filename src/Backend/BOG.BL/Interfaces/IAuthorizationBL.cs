namespace BOG.BL.Interfaces;

/// <summary>
/// Authorization business logic interface.
/// </summary>
public interface IAuthorizationBL
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions.
    /// </summary>
    Task<bool> HasAnyPermissionAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has all of the specified permissions.
    /// </summary>
    Task<bool> HasAllPermissionsAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    Task<bool> HasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified roles.
    /// </summary>
    Task<bool> HasAnyRoleAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a user based on their roles.
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}
