using BOG.BL.Constants;
using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;

namespace BOG.BL.Services;

/// <summary>
/// Authorization business logic service.
/// </summary>
public class AuthorizationBL : IAuthorizationBL
{
    private readonly IRoleRepository _roleRepository;

    // Role-Permission mapping
    private static readonly Dictionary<string, HashSet<string>> RolePermissions = new()
    {
        ["Clerk"] = new HashSet<string>
        {
            Permissions.CreateRequest,
            Permissions.ViewRequest,
            Permissions.EditRequest,
            Permissions.SubmitRequest
        },
        ["Reviewer"] = new HashSet<string>
        {
            Permissions.ViewRequest,
            Permissions.RegisterCase,
            Permissions.RejectRequest,
            Permissions.SendToJudge,
            Permissions.RequestCompletion
        },
        ["Judge"] = new HashSet<string>
        {
            Permissions.ViewRequest,
            Permissions.RegisterCase,
            Permissions.RejectRequest
        },
        ["RegistrationEmployee"] = new HashSet<string>
        {
            Permissions.CreateRequest,
            Permissions.ViewRequest,
            Permissions.RegisterCase
        },
        ["StatusEmployee"] = new HashSet<string>
        {
            Permissions.ViewRequest,
            Permissions.UpdateStatus
        },
        ["Admin"] = new HashSet<string>
        {
            Permissions.CreateRequest,
            Permissions.ViewRequest,
            Permissions.EditRequest,
            Permissions.SubmitRequest,
            Permissions.RegisterCase,
            Permissions.RejectRequest,
            Permissions.SendToJudge,
            Permissions.RequestCompletion,
            Permissions.UpdateStatus
        }
    };

    public AuthorizationBL(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be null or empty.", nameof(permission));

        var userRoles = await _roleRepository.GetUserRolesAsync(userId, cancellationToken);

        foreach (var role in userRoles)
        {
            if (RolePermissions.TryGetValue(role.Name, out var permissions) && permissions.Contains(permission))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> HasAnyPermissionAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        if (permissions == null || !permissions.Any())
            throw new ArgumentException("Permissions cannot be null or empty.", nameof(permissions));

        var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
        return permissions.Any(p => userPermissions.Contains(p));
    }

    public async Task<bool> HasAllPermissionsAsync(int userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        if (permissions == null || !permissions.Any())
            throw new ArgumentException("Permissions cannot be null or empty.", nameof(permissions));

        var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
        return permissions.All(p => userPermissions.Contains(p));
    }

    public async Task<bool> HasRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));

        var userRoles = await _roleRepository.GetUserRolesAsync(userId, cancellationToken);
        return userRoles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> HasAnyRoleAsync(int userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        if (roleNames == null || !roleNames.Any())
            throw new ArgumentException("Role names cannot be null or empty.", nameof(roleNames));

        var userRoles = await _roleRepository.GetUserRolesAsync(userId, cancellationToken);
        var userRoleNames = userRoles.Select(r => r.Name.ToLowerInvariant()).ToHashSet();

        return roleNames.Any(rn => userRoleNames.Contains(rn.ToLowerInvariant()));
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        var userRoles = await _roleRepository.GetUserRolesAsync(userId, cancellationToken);
        var permissions = new HashSet<string>();

        foreach (var role in userRoles)
        {
            if (RolePermissions.TryGetValue(role.Name, out var rolePerms))
            {
                foreach (var perm in rolePerms)
                {
                    permissions.Add(perm);
                }
            }
        }

        return permissions;
    }
}
