using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Repository implementation for CaseRegistrationRequest entity.
/// Handles complex queries for request state management and status filtering.
/// Follows Repository Pattern and SOLID principles.
/// </summary>
public class CaseRegistrationRequestRepository : Repository<CaseRegistrationRequest>, ICaseRegistrationRequestRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public CaseRegistrationRequestRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets a case registration request with all related navigation properties loaded.
    /// Includes:
    /// - Plaintiffs (via CaseRequestPlaintiff junction)
    /// - Defendants (via CaseRequestDefendant junction)
    /// - Claims
    /// - Attachments
    /// - Classifications
    /// - Court and Status lookups
    ///
    /// Returns null if request not found or is soft-deleted.
    /// </summary>
    public async Task<CaseRegistrationRequest?> GetWithDetailsAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var request = await _dbSet
            .Include(r => r.Status)
            .Include(r => r.Court)
            .Include(r => r.CaseRequestPlaintiffs)
                .ThenInclude(cp => cp.Plaintiff)
            .Include(r => r.CaseRequestDefendants)
                .ThenInclude(cd => cd.Defendant)
            .Include(r => r.Claims)
            .Include(r => r.Attachments)
            .Include(r => r.Classifications.Where(c => !c.IsDeleted))
                .ThenInclude(rc => rc.Classification)
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted, cancellationToken);

        if (request == null)
            return null;

        return request;
    }

    /// <summary>
    /// Gets all requests with a specific status.
    /// Useful for dashboard queries (Get all drafts, pending, etc.)
    /// Automatically filters soft-deleted records.
    /// Orders by most recent first.
    /// </summary>
    public async Task<IEnumerable<CaseRegistrationRequest>> GetByStatusAsync(int statusId, CancellationToken cancellationToken = default)
    {
        var requests = await _dbSet
            .Where(r => r.RequestStatusId == statusId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);

        // Load navigation properties separately
        foreach (var request in requests)
        {
            await _applicationDbContext.Entry(request)
                .Reference(r => r.Status)
                .LoadAsync(cancellationToken);
            await _applicationDbContext.Entry(request)
                .Reference(r => r.Court)
                .LoadAsync(cancellationToken);
        }

        return requests;
    }

    /// <summary>
    /// Gets all requests in PendingCompletion status (8) where completion deadline has expired.
    /// Used for BR05 (auto-rejection after 30 days).
    ///
    /// Returns requests where:
    /// - RequestStatusId = 8 (PendingCompletion)
    /// - CompletionDeadline < DateTime.UtcNow
    /// - IsDeleted = false
    ///
    /// Includes related plaintiffs to send notifications.
    /// </summary>
    public async Task<IEnumerable<CaseRegistrationRequest>> GetPendingCompletionExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.CaseRequestPlaintiffs)
                .ThenInclude(cp => cp.Plaintiff)
            .Where(r => r.RequestStatusId == 8 // PendingCompletion
                     && r.CompletionDeadline < DateTime.UtcNow
                     && !r.IsDeleted)
            .OrderBy(r => r.CompletionDeadline)
            .ToListAsync(cancellationToken);
    }
}
