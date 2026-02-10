using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Repository implementation for Defendant entity.
/// Follows Repository Pattern and SOLID principles.
/// Handles soft delete filtering automatically.
/// </summary>
public class DefendantRepository : Repository<Defendant>, IDefendantRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public DefendantRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets all defendants for a specific case registration request.
    /// Includes related DefendantType and IdentityType lookups.
    /// Automatically filters soft-deleted records.
    /// </summary>
    public async Task<IEnumerable<Defendant>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        // Get defendant IDs for this request from junction table
        var defendantIds = await _applicationDbContext.CaseRequestDefendants
            .Where(cd => cd.CaseRegistrationRequestId == requestId)
            .Select(cd => cd.DefendantId)
            .ToListAsync(cancellationToken);

        if (!defendantIds.Any())
            return new List<Defendant>();

        // Query defendants by those IDs
        var defendants = await _dbSet
            .Where(d => defendantIds.Contains(d.Id) && !d.IsDeleted)
            .OrderBy(d => d.CreatedDate)
            .ToListAsync(cancellationToken);

        // Load navigation properties separately
        foreach (var defendant in defendants)
        {
            await _applicationDbContext.Entry(defendant)
                .Reference(d => d.DefendantType)
                .LoadAsync(cancellationToken);
            await _applicationDbContext.Entry(defendant)
                .Reference(d => d.IdentityType)
                .LoadAsync(cancellationToken);
        }

        return defendants;
    }

    /// <summary>
    /// Gets a defendant by identity number within a request (for duplicate checking - ERR013).
    /// Returns null if:
    /// - Identity number is empty/null
    /// - Defendant doesn't exist
    /// - Defendant is soft-deleted
    /// </summary>
    public async Task<Defendant?> GetByIdentityAsync(int requestId, string identityNumber, int defendantTypeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            return null;

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.CaseRequestDefendants.Any(cd => cd.CaseRegistrationRequestId == requestId)
                                    && d.IdentityNumber == identityNumber
                                    && d.DefendantTypeId == defendantTypeId
                                    && !d.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Checks if a defendant with the same identity already exists in the request (ERR013 - Duplicate Check).
    /// Returns false if identity number is null/empty.
    /// </summary>
    public async Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int defendantTypeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            return false;

        // Use LINQ join to find defendants linked to this request with matching identity
        var exists = await _applicationDbContext.CaseRequestDefendants
            .AsNoTracking()
            .Where(cd => cd.CaseRegistrationRequestId == requestId)
            .Join(_dbSet.AsNoTracking(),
                cd => cd.DefendantId,
                d => d.Id,
                (cd, d) => d)
            .AnyAsync(d => d.IdentityNumber == identityNumber
                        && d.DefendantTypeId == defendantTypeId
                        && !d.IsDeleted,
                cancellationToken);

        return exists;
    }
}
