using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// CaseRequestDefendant junction table repository implementation.
/// </summary>
public class CaseRequestDefendantRepository : Repository<CaseRequestDefendant>, ICaseRequestDefendantRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CaseRequestDefendantRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<CaseRequestDefendant>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(crd => crd.Defendant)
                .ThenInclude(d => d.DefendantType)
            .Where(crd => crd.CaseRegistrationRequestId == requestId && !crd.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int requestId, int defendantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(crd =>
                crd.CaseRegistrationRequestId == requestId &&
                crd.DefendantId == defendantId &&
                !crd.IsDeleted,
                cancellationToken);
    }
}
