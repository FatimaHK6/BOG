using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Defendant repository implementation.
/// </summary>
public class DefendantRepository : Repository<Defendant>, IDefendantRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public DefendantRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<Defendant>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        // Get defendant IDs for this request
        var defendantIds = await _applicationDbContext.Set<CaseRequestDefendant>()
            .AsNoTracking()
            .Where(crd => crd.CaseRegistrationRequestId == requestId && !crd.IsDeleted)
            .Select(crd => crd.DefendantId)
            .ToListAsync(cancellationToken);

        // Fetch defendants with navigation properties
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.DefendantType)
            .Include(d => d.GovernmentAgency)
            .Where(d => defendantIds.Contains(d.Id) && !d.IsDeleted && d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Defendant?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(d => d.DefendantType)
            .Include(d => d.GovernmentAgency)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsDuplicateAsync(int requestId, string identityNumber, int defendantTypeId, CancellationToken cancellationToken = default)
    {
        // Get defendant IDs for this request
        var defendantIds = await _applicationDbContext.Set<CaseRequestDefendant>()
            .AsNoTracking()
            .Where(crd => crd.CaseRegistrationRequestId == requestId && !crd.IsDeleted)
            .Select(crd => crd.DefendantId)
            .ToListAsync(cancellationToken);

        // Check if any defendant with same identity number and type exists
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(d => defendantIds.Contains(d.Id)
                && !d.IsDeleted
                && d.IsActive
                && d.IdentityNumber == identityNumber
                && d.DefendantTypeId == defendantTypeId, cancellationToken);
    }

    public async Task<bool> ExistsDuplicateExcludingAsync(int requestId, string identityNumber, int defendantTypeId, int excludeDefendantId, CancellationToken cancellationToken = default)
    {
        // Get defendant IDs for this request
        var defendantIds = await _applicationDbContext.Set<CaseRequestDefendant>()
            .AsNoTracking()
            .Where(crd => crd.CaseRegistrationRequestId == requestId && !crd.IsDeleted)
            .Select(crd => crd.DefendantId)
            .ToListAsync(cancellationToken);

        // Check if any OTHER defendant with same identity number and type exists
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(d => defendantIds.Contains(d.Id)
                && d.Id != excludeDefendantId
                && !d.IsDeleted
                && d.IsActive
                && d.IdentityNumber == identityNumber
                && d.DefendantTypeId == defendantTypeId, cancellationToken);
    }

    public async Task<int?> GetRequestIdAsync(int defendantId, CancellationToken cancellationToken = default)
    {
        var caseRequestDefendant = await _applicationDbContext.Set<CaseRequestDefendant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(crd => crd.DefendantId == defendantId && !crd.IsDeleted, cancellationToken);

        return caseRequestDefendant?.CaseRegistrationRequestId;
    }
}
