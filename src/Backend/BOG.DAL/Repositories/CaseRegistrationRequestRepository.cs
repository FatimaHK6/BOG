using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// CaseRegistrationRequest repository implementation.
/// </summary>
public class CaseRegistrationRequestRepository : Repository<CaseRegistrationRequest>, ICaseRegistrationRequestRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CaseRegistrationRequestRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public override async Task<IEnumerable<CaseRegistrationRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .Include(r => r.Status)
            .Include(r => r.Court)
            .Include(r => r.CaseRequestPlaintiffs.Where(p => !p.IsDeleted))
                .ThenInclude(crp => crp.Plaintiff)
                    .ThenInclude(p => p.PlaintiffType)
            .Include(r => r.CaseRequestDefendants.Where(d => !d.IsDeleted))
                .ThenInclude(crd => crd.Defendant)
            .OrderByDescending(r => r.ModifiedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CaseRegistrationRequest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.CreatedByUserId == userId && !r.IsDeleted)
            .Include(r => r.Status)
            .Include(r => r.Court)
            .Include(r => r.CaseRequestPlaintiffs.Where(p => !p.IsDeleted))
                .ThenInclude(crp => crp.Plaintiff)
                    .ThenInclude(p => p.PlaintiffType)
            .Include(r => r.CaseRequestDefendants.Where(d => !d.IsDeleted))
                .ThenInclude(crd => crd.Defendant)
            .OrderByDescending(r => r.ModifiedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<CaseRegistrationRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.Id == id && !r.IsDeleted)
            .Include(r => r.Status)
            .Include(r => r.Court)
            .Include(r => r.CreatedByUser)
            .Include(r => r.CaseRequestPlaintiffs.Where(p => !p.IsDeleted))
                .ThenInclude(crp => crp.Plaintiff)
                    .ThenInclude(p => p.PlaintiffType)
            .Include(r => r.CaseRequestDefendants.Where(d => !d.IsDeleted))
                .ThenInclude(crd => crd.Defendant)
            .Include(r => r.Claims.Where(c => !c.IsDeleted))
            .Include(r => r.Attachments.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<CaseRegistrationRequest>> GetPendingCompletionExpiredAsync(CancellationToken cancellationToken = default)
    {
        // Get requests in PendingCompletion status (ID: 6) that have exceeded their completion deadline
        // Deadline is 7 days from when the request entered PendingCompletion status
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        return await _dbSet
            .AsNoTracking()
            .Where(r => r.RequestStatusId == 6 && !r.IsDeleted && r.ModifiedDate <= sevenDaysAgo)
            .Include(r => r.Status)
            .Include(r => r.Court)
            .OrderByDescending(r => r.ModifiedDate)
            .ToListAsync(cancellationToken);
    }
}
