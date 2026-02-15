using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Repository implementation for RequestDeficiency entity.
/// Handles deficiencies (نواقص الدعوى) for case registration requests.
/// Supports soft delete with proper filtering.
/// </summary>
public class DeficiencyRepository : Repository<RequestDeficiency>, IDeficiencyRepository
{
    public DeficiencyRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Gets all deficiencies for a specific request.
    /// Filters soft-deleted records.
    /// Orders by display order.
    /// </summary>
    public async Task<IEnumerable<RequestDeficiency>> GetByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(d => d.DeficiencyDescription)
                .ThenInclude(dd => dd.DeficiencyType)
            .Where(d => d.CaseRegistrationRequestId == requestId && !d.IsDeleted)
            .OrderBy(d => d.DisplayOrder)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Soft deletes all deficiencies for a specific request.
    /// Used when replacing deficiencies in batch update.
    /// Updates ModifiedDate to track when deletion occurred.
    /// </summary>
    public async Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default)
    {
        var deficiencies = await _dbSet
            .Where(d => d.CaseRegistrationRequestId == requestId && !d.IsDeleted)
            .ToListAsync(ct);

        foreach (var deficiency in deficiencies)
        {
            deficiency.IsDeleted = true;
            deficiency.ModifiedDate = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
