using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Repository implementation for RequestAttachment entity.
/// Handles request-level attachments (PDFs, documents, etc.).
/// Follows Repository Pattern and SOLID principles.
/// </summary>
public class RequestAttachmentRepository : Repository<RequestAttachment>, IRequestAttachmentRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public RequestAttachmentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets all attachments for a specific case registration request.
    /// Includes related AttachmentType lookup.
    /// Automatically filters soft-deleted records.
    /// Orders by upload date (newest first).
    /// </summary>
    public async Task<IEnumerable<RequestAttachment>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var attachments = await _dbSet
            .Where(a => a.CaseRegistrationRequestId == requestId && !a.IsDeleted)
            .OrderByDescending(a => a.UploadDate)
            .ToListAsync(cancellationToken);

        // Load navigation properties separately
        foreach (var attachment in attachments)
        {
            await _applicationDbContext.Entry(attachment)
                .Reference(a => a.AttachmentType)
                .LoadAsync(cancellationToken);
        }

        return attachments;
    }
}
