using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// PlaintiffAttachment repository implementation.
/// Follows SOLID principles - handles only plaintiff attachment data access.
/// </summary>
public class PlaintiffAttachmentRepository : Repository<PlaintiffAttachment>, IPlaintiffAttachmentRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public PlaintiffAttachmentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<PlaintiffAttachment>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.AttachmentType)
            .Where(a => a.PlaintiffId == plaintiffId && !a.IsDeleted && a.IsActive)
            .OrderByDescending(a => a.UploadDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlaintiffAttachment>> GetByTypeAsync(int plaintiffId, int attachmentTypeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(a => a.AttachmentType)
            .Where(a =>
                a.PlaintiffId == plaintiffId &&
                a.AttachmentTypeId == attachmentTypeId &&
                !a.IsDeleted &&
                a.IsActive)
            .OrderByDescending(a => a.UploadDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasRequiredAttachmentsAsync(int plaintiffId, IEnumerable<int> requiredAttachmentTypeIds, CancellationToken cancellationToken = default)
    {
        var existingTypeIds = await _dbSet
            .AsNoTracking()
            .Where(a => a.PlaintiffId == plaintiffId && !a.IsDeleted && a.IsActive)
            .Select(a => a.AttachmentTypeId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return requiredAttachmentTypeIds.All(requiredId => existingTypeIds.Contains(requiredId));
    }
}
