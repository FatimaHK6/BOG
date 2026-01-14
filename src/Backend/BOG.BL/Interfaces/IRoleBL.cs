using BOG.DTO.Role;
using BOG.VM.Role;

namespace BOG.BL.Interfaces;

/// <summary>
/// Role business logic interface.
/// </summary>
public interface IRoleBL
{
    /// <summary>
    /// Creates a new role.
    /// </summary>
    Task<RoleVM> CreateRoleAsync(RoleCreateDTO createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    Task<RoleVM?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<RoleVM?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<IEnumerable<RoleVM>> GetAllActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a role.
    /// </summary>
    Task<RoleVM> UpdateRoleAsync(int roleId, RoleUpdateDTO updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role (soft delete).
    /// </summary>
    Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets roles assigned to a user.
    /// </summary>
    Task<IEnumerable<RoleVM>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    Task AssignRoleToUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    Task RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default);
}
