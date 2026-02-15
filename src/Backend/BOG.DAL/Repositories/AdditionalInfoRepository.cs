using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Repository implementation for AdditionalInfo entity.
/// Provides data access operations for additional information related to case registration requests.
/// </summary>
public class AdditionalInfoRepository : Repository<AdditionalInfo>, IAdditionalInfoRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public AdditionalInfoRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets additional information by case registration request ID with all type-specific relationships (read-only).
    /// Returns null if not found or if soft deleted.
    /// Uses AsNoTracking for read-only queries to improve performance.
    /// </summary>
    public async Task<AdditionalInfo?> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Request ID must be greater than 0.", nameof(requestId));

        return await _dbSet
            .Where(ai => ai.CaseRegistrationRequestId == requestId && !ai.IsDeleted)
            .Include(ai => ai.ManagementDecision)
                .ThenInclude(md => md.NotificationMethod)
            .Include(ai => ai.ManagementDecision)
                .ThenInclude(md => md.IssuingAuthority)
            .Include(ai => ai.ServiceRights)
                .ThenInclude(sr => sr.ComplaintAuthority)
            .Include(ai => ai.Trademark)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets additional information by case registration request ID with tracking enabled.
    /// Used for update operations to ensure nested navigation property changes are tracked.
    /// Returns null if not found or if soft deleted.
    /// IMPORTANT: Use this only for update operations, not for read-only queries.
    /// </summary>
    public async Task<AdditionalInfo?> GetByRequestIdAsyncForUpdateAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Request ID must be greater than 0.", nameof(requestId));

        return await _dbSet
            .Include(ai => ai.ManagementDecision)
                .ThenInclude(md => md.NotificationMethod)
            .Include(ai => ai.ManagementDecision)
                .ThenInclude(md => md.IssuingAuthority)
            .Include(ai => ai.ServiceRights)
                .ThenInclude(sr => sr.ComplaintAuthority)
            .Include(ai => ai.Trademark)
            // NOTE: No AsNoTracking() - entities are tracked for update operations
            .FirstOrDefaultAsync(
                ai => ai.CaseRegistrationRequestId == requestId && !ai.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Checks if additional information exists for a request.
    /// </summary>
    public async Task<bool> ExistsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Request ID must be greater than 0.", nameof(requestId));

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(
                ai => ai.CaseRegistrationRequestId == requestId && !ai.IsDeleted,
                cancellationToken);
    }
}
