using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Court repository implementation.
/// </summary>
public class CourtRepository : Repository<Court>, ICourtRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CourtRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<Court>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.NameAr)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Court>> GetByRegionAsync(int regionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.RegionId == regionId && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.NameAr)
            .ToListAsync(cancellationToken);
    }

    public async Task<Court?> GetWithDepartmentsAsync(int courtId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.Departments.Where(d => d.IsActive && !d.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == courtId && !c.IsDeleted, cancellationToken);
    }
}
