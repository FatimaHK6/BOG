using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.Identity;
using BOG.DTO.Role;
using BOG.VM.Role;

namespace BOG.BL.Services;

/// <summary>
/// Role business logic service.
/// </summary>
public class RoleBL : IRoleBL
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoleBL(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<RoleVM> CreateRoleAsync(RoleCreateDTO createDto, CancellationToken cancellationToken = default)
    {
        if (createDto == null)
            throw new ArgumentNullException(nameof(createDto));

        // Check if role name already exists
        var existingRole = await _roleRepository.GetByNameAsync(createDto.Name, cancellationToken);
        if (existingRole != null)
            throw new InvalidOperationException($"A role with name '{createDto.Name}' already exists.");

        var role = new Role
        {
            Name = createDto.Name,
            NameAr = createDto.NameAr,
            Description = createDto.Description,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await _roleRepository.AddAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        return MapToViewModel(role);
    }

    public async Task<RoleVM?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than 0.", nameof(roleId));

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);

        if (role == null || role.IsDeleted)
            return null;

        return MapToViewModel(role);
    }

    public async Task<RoleVM?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        var role = await _roleRepository.GetByNameAsync(name, cancellationToken);

        if (role == null)
            return null;

        return MapToViewModel(role);
    }

    public async Task<IEnumerable<RoleVM>> GetAllActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllActiveAsync(cancellationToken);
        return roles.Select(MapToViewModel).ToList();
    }

    public async Task<RoleVM> UpdateRoleAsync(int roleId, RoleUpdateDTO updateDto, CancellationToken cancellationToken = default)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than 0.", nameof(roleId));

        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto));

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);

        if (role == null || role.IsDeleted)
            throw new InvalidOperationException($"Role with ID {roleId} not found.");

        if (!string.IsNullOrWhiteSpace(updateDto.Name))
            role.Name = updateDto.Name;

        if (!string.IsNullOrWhiteSpace(updateDto.NameAr))
            role.NameAr = updateDto.NameAr;

        if (updateDto.Description != null)
            role.Description = updateDto.Description;

        if (updateDto.IsActive.HasValue)
            role.IsActive = updateDto.IsActive.Value;

        role.ModifiedDate = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        return MapToViewModel(role);
    }

    public async Task DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than 0.", nameof(roleId));

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);

        if (role == null || role.IsDeleted)
            throw new InvalidOperationException($"Role with ID {roleId} not found.");

        role.IsDeleted = true;
        role.ModifiedDate = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<RoleVM>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        var roles = await _roleRepository.GetUserRolesAsync(userId, cancellationToken);
        return roles.Select(MapToViewModel).ToList();
    }

    public Task AssignRoleToUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        // This will be implemented when we have the UserRole repository
        throw new NotImplementedException("UserRole repository needs to be injected.");
    }

    public Task RemoveRoleFromUserAsync(int userId, int roleId, CancellationToken cancellationToken = default)
    {
        // This will be implemented when we have the UserRole repository
        throw new NotImplementedException("UserRole repository needs to be injected.");
    }

    private static RoleVM MapToViewModel(Role role)
    {
        return new RoleVM
        {
            Id = role.Id,
            Name = role.Name,
            NameAr = role.NameAr,
            Description = role.Description,
            IsActive = role.IsActive,
            CreatedDate = role.CreatedDate
        };
    }
}
