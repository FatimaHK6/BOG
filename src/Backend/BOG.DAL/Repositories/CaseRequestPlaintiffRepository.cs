using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// CaseRequestPlaintiff junction table repository implementation.
/// Manages the many-to-many relationship between CaseRegistrationRequest and Plaintiff.
/// </summary>
public class CaseRequestPlaintiffRepository : Repository<CaseRequestPlaintiff>, ICaseRequestPlaintiffRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CaseRequestPlaintiffRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<CaseRequestPlaintiff>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(crp => crp.Plaintiff)
                .ThenInclude(p => p.PlaintiffType)
            .Where(crp => crp.CaseRegistrationRequestId == requestId && !crp.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CaseRequestPlaintiff>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(crp => crp.Request)
            .Where(crp => crp.PlaintiffId == plaintiffId && !crp.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int requestId, int plaintiffId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(crp =>
                crp.CaseRegistrationRequestId == requestId &&
                crp.PlaintiffId == plaintiffId &&
                !crp.IsDeleted,
                cancellationToken);
    }

    public async Task<CaseRequestPlaintiff?> GetByRequestAndPlaintiffAsync(int requestId, int plaintiffId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(crp =>
                crp.CaseRegistrationRequestId == requestId &&
                crp.PlaintiffId == plaintiffId &&
                !crp.IsDeleted,
                cancellationToken);
    }

    public async Task<int> GetPlaintiffCountByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(crp => crp.CaseRegistrationRequestId == requestId && !crp.IsDeleted, cancellationToken);
    }
}
