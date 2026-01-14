using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Role repository implementation.
/// </summary>
public class RoleRepository : Repository<Role>, IRoleRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public RoleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == name && !r.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _applicationDbContext.Set<UserRole>()
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && !ur.IsDeleted)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .Where(r => r.IsActive && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
