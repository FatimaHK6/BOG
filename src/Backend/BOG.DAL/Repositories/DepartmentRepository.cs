using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Department repository implementation.
/// </summary>
public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public DepartmentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<Department>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.IsActive && !d.IsDeleted)
            .Include(d => d.Court)
            .OrderBy(d => d.NameAr)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetByCourtAsync(int courtId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.CourtId == courtId && d.IsActive && !d.IsDeleted)
            .OrderBy(d => d.NameAr)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetUserDepartmentsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _applicationDbContext.Set<UserDepartment>()
            .AsNoTracking()
            .Where(ud => ud.UserId == userId && !ud.IsDeleted)
            .Include(ud => ud.Department)
            .ThenInclude(d => d.Court)
            .Select(ud => ud.Department)
            .Where(d => d.IsActive && !d.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
