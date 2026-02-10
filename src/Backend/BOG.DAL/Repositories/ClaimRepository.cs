using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Repository implementation for Claim entity.
/// Handles claims (طلبات الدعوى) for case registration requests.
/// </summary>
public class ClaimRepository : Repository<Claim>, IClaimRepository
{
    public ClaimRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Gets all claims for a specific request.
    /// Filters soft-deleted records.
    /// Orders by creation date (oldest first).
    /// </summary>
    public async Task<IEnumerable<Claim>> GetByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.CaseRegistrationRequestId == requestId && !c.IsDeleted)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Soft deletes all claims for a specific request.
    /// Used when replacing claims in batch update.
    /// </summary>
    public async Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        var claims = await _dbSet
            .Where(c => c.CaseRegistrationRequestId == requestId && !c.IsDeleted)
            .ToListAsync(ct);

        foreach (var claim in claims)
        {
            claim.IsDeleted = true;
            claim.ModifiedDate = DateTime.UtcNow;
        }
    }
}
