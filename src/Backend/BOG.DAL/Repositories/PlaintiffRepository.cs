using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Plaintiff repository implementation.
/// Follows SOLID principles - handles only plaintiff data access.
/// </summary>
public class PlaintiffRepository : Repository<Plaintiff>, IPlaintiffRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public PlaintiffRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<Plaintiff>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return await _applicationDbContext.Set<CaseRequestPlaintiff>()
            .AsNoTracking()
            .Where(crp => crp.CaseRegistrationRequestId == requestId && !crp.IsDeleted)
            .Include(crp => crp.Plaintiff)
                .ThenInclude(p => p.PlaintiffType)
            .Include(crp => crp.Plaintiff)
                .ThenInclude(p => p.DataSource)
            .Select(crp => crp.Plaintiff)
            .Where(p => !p.IsDeleted && p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Plaintiff?> GetByIdentityAsync(int requestId, string identityNumber, int plaintiffTypeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            return null;

        var plaintiffIds = await _applicationDbContext.Set<CaseRequestPlaintiff>()
            .AsNoTracking()
            .Where(crp => crp.CaseRegistrationRequestId == requestId && !crp.IsDeleted)
            .Select(crp => crp.PlaintiffId)
            .ToListAsync(cancellationToken);

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                plaintiffIds.Contains(p.Id) &&
                p.IdentityNumber == identityNumber &&
                p.PlaintiffTypeId == plaintiffTypeId &&
                !p.IsDeleted &&
                p.IsActive,
                cancellationToken);
    }

    public async Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int plaintiffTypeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
            return false;

        var plaintiffIds = await _applicationDbContext.Set<CaseRequestPlaintiff>()
            .AsNoTracking()
            .Where(crp => crp.CaseRegistrationRequestId == requestId && !crp.IsDeleted)
            .Select(crp => crp.PlaintiffId)
            .ToListAsync(cancellationToken);

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(p =>
                plaintiffIds.Contains(p.Id) &&
                p.IdentityNumber == identityNumber &&
                p.PlaintiffTypeId == plaintiffTypeId &&
                !p.IsDeleted &&
                p.IsActive,
                cancellationToken);
    }

    public async Task<bool> ExistsByDocumentNumberAsync(int requestId, string documentNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            return false;

        var plaintiffIds = await _applicationDbContext.Set<CaseRequestPlaintiff>()
            .AsNoTracking()
            .Where(crp => crp.CaseRegistrationRequestId == requestId && !crp.IsDeleted)
            .Select(crp => crp.PlaintiffId)
            .ToListAsync(cancellationToken);

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(p =>
                plaintiffIds.Contains(p.Id) &&
                p.DocumentNumber == documentNumber &&
                p.PlaintiffTypeId == 2 && // Type 2: Individual without ID
                !p.IsDeleted &&
                p.IsActive,
                cancellationToken);
    }

    public async Task<Plaintiff?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.PlaintiffType)
            .Include(p => p.IdentityType)
            .Include(p => p.DataSource)
            .Include(p => p.GovernmentAgency)
            .Include(p => p.LicenseSource)
            .Include(p => p.Country)
            // Address navigation properties with Region and City
            .Include(p => p.ResidenceAddress)
                .ThenInclude(a => a!.Region)
            .Include(p => p.ResidenceAddress)
                .ThenInclude(a => a!.City_)
            .Include(p => p.WorkAddress)
                .ThenInclude(a => a!.Region)
            .Include(p => p.WorkAddress)
                .ThenInclude(a => a!.City_)
            .Include(p => p.BusinessAddress)
                .ThenInclude(a => a!.Region)
            .Include(p => p.BusinessAddress)
                .ThenInclude(a => a!.City_)
            .Include(p => p.CompanyAddress)
                .ThenInclude(a => a!.Region)
            .Include(p => p.CompanyAddress)
                .ThenInclude(a => a!.City_)
            .Include(p => p.NGOAddress)
                .ThenInclude(a => a!.Region)
            .Include(p => p.NGOAddress)
                .ThenInclude(a => a!.City_)
            .Include(p => p.WaqfAddress)
                .ThenInclude(a => a!.Region)
            .Include(p => p.WaqfAddress)
                .ThenInclude(a => a!.City_)
            .Include(p => p.SelectedAddress)
                .ThenInclude(a => a!.Region)
            .Include(p => p.SelectedAddress)
                .ThenInclude(a => a!.City_)
            // Representatives
            .Include(p => p.Representatives.Where(r => !r.IsDeleted && r.IsActive))
                .ThenInclude(r => r.RepresentativeType)
            .Include(p => p.Representatives.Where(r => !r.IsDeleted && r.IsActive))
                .ThenInclude(r => r.DataSource)
            .Include(p => p.Attachments.Where(a => !a.IsDeleted && a.IsActive))
                .ThenInclude(a => a.AttachmentType)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && p.IsActive, cancellationToken);
    }

    public async Task<Plaintiff?> GetApplicantByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var plaintiffIds = await _applicationDbContext.Set<CaseRequestPlaintiff>()
            .AsNoTracking()
            .Where(crp => crp.CaseRegistrationRequestId == requestId && !crp.IsDeleted)
            .Select(crp => crp.PlaintiffId)
            .ToListAsync(cancellationToken);

        return await _dbSet
            .AsNoTracking()
            .Include(p => p.PlaintiffType)
            .FirstOrDefaultAsync(p =>
                plaintiffIds.Contains(p.Id) &&
                p.IsApplicant &&
                !p.IsDeleted &&
                p.IsActive,
                cancellationToken);
    }
}
