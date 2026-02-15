using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Repository implementation for RelatedCase entity.
/// Handles related cases (الدعاوى المرتبطة) for case registration requests.
/// </summary>
public class RelatedCaseRepository : Repository<RelatedCase>, IRelatedCaseRepository
{
    public RelatedCaseRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Gets all related cases for a specific request.
    /// Includes Court navigation property for display.
    /// Filters soft-deleted records.
    /// Orders by creation date (newest first).
    /// </summary>
    public async Task<IEnumerable<RelatedCase>> GetByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(rc => rc.Court)
            .Where(rc => rc.CaseRegistrationRequestId == requestId && !rc.IsDeleted)
            .OrderByDescending(rc => rc.CreatedDate)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Soft deletes all related cases for a specific request.
    /// Used when replacing related cases in batch update.
    /// </summary>
    public async Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        var relatedCases = await _dbSet
            .Where(rc => rc.CaseRegistrationRequestId == requestId && !rc.IsDeleted)
            .ToListAsync(ct);

        foreach (var relatedCase in relatedCases)
        {
            relatedCase.IsDeleted = true;
            relatedCase.ModifiedDate = DateTime.UtcNow;
        }
    }
}
