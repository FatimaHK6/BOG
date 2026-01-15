using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Representative repository implementation.
/// Follows SOLID principles - handles only representative data access.
/// </summary>
public class RepresentativeRepository : Repository<Representative>, IRepresentativeRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public RepresentativeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<Representative>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.RepresentativeType)
            .Include(r => r.IdentityType)
            .Include(r => r.DataSource)
            .Where(r => r.PlaintiffId == plaintiffId && !r.IsDeleted && r.IsActive)
            .OrderBy(r => r.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Representative?> GetByIdentityAsync(int plaintiffId, string identityNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            return null;

        return await _dbSet
            .AsNoTracking()
            .Include(r => r.RepresentativeType)
            .FirstOrDefaultAsync(r =>
                r.PlaintiffId == plaintiffId &&
                r.IdentityNumber == identityNumber &&
                !r.IsDeleted &&
                r.IsActive,
                cancellationToken);
    }

    public async Task<bool> ExistsByIdentityAsync(int plaintiffId, string identityNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            return false;

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(r =>
                r.PlaintiffId == plaintiffId &&
                r.IdentityNumber == identityNumber &&
                !r.IsDeleted &&
                r.IsActive,
                cancellationToken);
    }

    public async Task<Representative?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.RepresentativeType)
            .Include(r => r.IdentityType)
            .Include(r => r.DataSource)
            .Include(r => r.Plaintiff)
                .ThenInclude(p => p.PlaintiffType)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && r.IsActive, cancellationToken);
    }
}
