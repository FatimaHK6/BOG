using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.VM.CaseRegistrationRequest;

namespace BOG.BL.Services;

/// <summary>
/// Business logic service for CaseRegistrationRequest.
/// </summary>
public class CaseRegistrationRequestBL : ICaseRegistrationRequestBL
{
    private readonly ICaseRegistrationRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    // Status constants
    private const int StatusDraft = 1;

    public CaseRegistrationRequestBL(
        ICaseRegistrationRequestRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<CaseRegistrationRequestListVM>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _repository.GetAllAsync(cancellationToken);
        return requests.Select(MapToListVM).ToList();
    }

    public async Task<IEnumerable<CaseRegistrationRequestListVM>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var requests = await _repository.GetByUserIdAsync(userId, cancellationToken);
        return requests.Select(MapToListVM).ToList();
    }

    public async Task<CaseRegistrationRequestVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetWithDetailsAsync(id, cancellationToken);
        return request == null ? null : MapToVM(request);
    }

    public async Task<CaseRegistrationRequestVM> CreateDraftAsync(int userId, CancellationToken cancellationToken = default)
    {
        var request = new CaseRegistrationRequest
        {
            RequestStatusId = StatusDraft,
            CaseTypeId = 1, // Default to first CaseType
            CreatedByUserId = userId,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await _repository.AddAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Retrieve the created request - if GetByIdAsync fails, create a minimal VM from the created entity
        var createdRequest = await GetByIdAsync(request.Id, cancellationToken);
        if (createdRequest != null)
        {
            return createdRequest;
        }

        // Fallback: Return minimal VM if detailed retrieval fails
        return new CaseRegistrationRequestVM
        {
            Id = request.Id,
            RequestStatusId = request.RequestStatusId,
            StatusNameAr = "مسودة",
            StatusName = "Draft",
            CreatedDate = request.CreatedDate,
            ModifiedDate = request.ModifiedDate,
            CreatedByUserId = request.CreatedByUserId
        };
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken) as CaseRegistrationRequest;
        if (request == null || request.IsDeleted)
            throw new InvalidOperationException($"Request {id} not found");

        request.IsDeleted = true;
        request.ModifiedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CaseRegistrationRequestVM?> UpdateAsync(int id, bool saveAsDraft, CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken) as CaseRegistrationRequest;
        if (request == null || request.IsDeleted)
            return null;

        // If saving as draft, ensure status is draft
        if (saveAsDraft)
        {
            request.RequestStatusId = StatusDraft;
        }

        request.ModifiedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private static CaseRegistrationRequestListVM MapToListVM(CaseRegistrationRequest request)
    {
        // Get first plaintiff name
        var firstPlaintiff = request.CaseRequestPlaintiffs?
            .Where(p => !p.IsDeleted && p.Plaintiff != null)
            .Select(p => p.Plaintiff)
            .FirstOrDefault();
        var plaintiffName = firstPlaintiff != null
            ? $"{firstPlaintiff.FirstName} {firstPlaintiff.FatherName} {firstPlaintiff.FamilyName}".Trim()
            : null;

        // Get first defendant name
        var firstDefendant = request.CaseRequestDefendants?
            .Where(d => !d.IsDeleted && d.Defendant != null)
            .Select(d => d.Defendant)
            .FirstOrDefault();
        var defendantName = firstDefendant?.FullName;

        return new CaseRegistrationRequestListVM
        {
            Id = request.Id,
            CaseNumber = request.CaseNumber,
            RegistrationNumber = request.RegistrationNumber,
            RequestStatusId = request.RequestStatusId,
            StatusName = request.Status?.Name ?? "",
            StatusNameAr = request.Status?.NameAr ?? "",
            CourtName = request.Court?.NameAr,
            CourtNameAr = request.Court?.NameAr,
            SubmissionDate = request.SubmissionDate,
            CreatedDate = request.CreatedDate,
            ModifiedDate = request.ModifiedDate,
            PlaintiffsCount = request.CaseRequestPlaintiffs?.Count(p => !p.IsDeleted) ?? 0,
            DefendantsCount = request.CaseRequestDefendants?.Count(d => !d.IsDeleted) ?? 0,
            // Additional fields for frontend
            IsDraft = request.RequestStatusId == StatusDraft,
            CaseTypeId = null, // TODO: Add CaseTypeId to entity when needed
            SubjectPreview = !string.IsNullOrEmpty(request.Subject)
                ? (request.Subject.Length > 50 ? request.Subject.Substring(0, 50) + "..." : request.Subject)
                : null,
            PlaintiffName = plaintiffName,
            DefendantName = defendantName
        };
    }

    private static CaseRegistrationRequestVM MapToVM(CaseRegistrationRequest request)
    {
        return new CaseRegistrationRequestVM
        {
            Id = request.Id,
            RequestStatusId = request.RequestStatusId,
            StatusName = request.Status?.Name ?? "",
            StatusNameAr = request.Status?.NameAr ?? "",
            Subject = request.Subject,
            Evidence = request.Evidence,
            Notes = request.Notes,
            CourtId = request.CourtId,
            CourtName = request.Court?.NameAr,
            SubmissionDate = request.SubmissionDate,
            CompletionDeadline = request.CompletionDeadline,
            RejectionReason = request.RejectionReason,
            CaseNumber = request.CaseNumber,
            RegistrationNumber = request.RegistrationNumber,
            RegistrationDate = request.RegistrationDate,
            CreatedByUserId = request.CreatedByUserId,
            CreatedByUserName = $"{request.CreatedByUser?.FirstName} {request.CreatedByUser?.LastName}".Trim(),
            CreatedDate = request.CreatedDate,
            ModifiedDate = request.ModifiedDate,
            PlaintiffsCount = request.CaseRequestPlaintiffs?.Count(p => !p.IsDeleted) ?? 0,
            DefendantsCount = request.CaseRequestDefendants?.Count(d => !d.IsDeleted) ?? 0,
            ClaimsCount = request.Claims?.Count(c => !c.IsDeleted) ?? 0,
            AttachmentsCount = request.Attachments?.Count(a => !a.IsDeleted) ?? 0
        };
    }
}
