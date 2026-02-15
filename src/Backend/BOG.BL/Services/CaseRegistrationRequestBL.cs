using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistrationRequest;
using Microsoft.EntityFrameworkCore;

namespace BOG.BL.Services;

/// <summary>
/// Business logic service for CaseRegistrationRequest.
/// </summary>
public class CaseRegistrationRequestBL : ICaseRegistrationRequestBL
{
    private readonly ICaseRegistrationRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    // Status constants (matching database seed in ApplicationDbContext.cs)
    private const int StatusDraft = 1;
    private const int StatusNew = 2;
    private const int StatusUnderReview = 3;
    private const int StatusRegistered = 4;
    private const int StatusRejected = 5;
    private const int StatusPendingCompletion = 6;
    private const int StatusOnJudgeDesk = 7;
    private const int StatusCompleted = 8;
    private const int StatusAutoRejected = 9;
    private const int StatusCancelled = 10;

    public CaseRegistrationRequestBL(
        ICaseRegistrationRequestRepository repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _context = context ?? throw new ArgumentNullException(nameof(context));
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

    public async Task<CaseRegistrationRequestVM?> UpdateAsync(
        int id,
        CaseRegistrationUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken) as CaseRegistrationRequest;

        if (request == null || request.IsDeleted)
            return null;

        // Only allow updates to Draft, New, or PendingCompletion requests
        if (request.RequestStatusId != StatusDraft && request.RequestStatusId != StatusNew && request.RequestStatusId != StatusPendingCompletion)
        {
            throw new InvalidOperationException(
                "لا يمكن تعديل الطلب إلا إذا كان في حالة مسودة أو طلب جديد أو بانتظار الاستكمال"
            );
        }

        // Update fields if provided
        if (dto.Subject != null)
            request.Subject = dto.Subject;

        if (dto.Evidence != null)
            request.Evidence = dto.Evidence;

        if (dto.CourtId.HasValue)
            request.CourtId = dto.CourtId.Value;

        if (dto.CaseTypeId.HasValue)
            request.CaseTypeId = dto.CaseTypeId.Value;

        if (dto.Notes != null)
            request.Notes = dto.Notes;

        if (dto.PrimaryMobile != null)
            request.PrimaryMobile = string.IsNullOrWhiteSpace(dto.PrimaryMobile) ? null : dto.PrimaryMobile.Trim();

        if (dto.SecondaryMobile != null)
            request.SecondaryMobile = string.IsNullOrWhiteSpace(dto.SecondaryMobile) ? null : dto.SecondaryMobile.Trim();

        if (dto.Email != null)
            request.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

        // Update classifications if provided
        if (dto.ClassificationIds != null)
        {
            // Load existing classifications (only non-deleted)
            var existingClassifications = await _context.RequestClassifications
                .Where(rc => rc.CaseRegistrationRequestId == id && !rc.IsDeleted)
                .ToListAsync(cancellationToken);

            var existingClassificationIds = existingClassifications
                .Select(rc => rc.ClassificationId)
                .ToHashSet();

            var newClassificationIds = dto.ClassificationIds.Distinct().ToHashSet();

            // Remove classifications that are no longer selected
            var classificationsToRemove = existingClassifications
                .Where(rc => !newClassificationIds.Contains(rc.ClassificationId))
                .ToList();

            if (classificationsToRemove.Any())
            {
                _context.RequestClassifications.RemoveRange(classificationsToRemove);
            }

            // Add new classifications that don't already exist
            var classificationsToAdd = newClassificationIds
                .Where(cid => !existingClassificationIds.Contains(cid))
                .Select(cid => new RequestClassification
                {
                    CaseRegistrationRequestId = id,
                    ClassificationId = cid,
                    DisplayOrder = 0
                })
                .ToList();

            if (classificationsToAdd.Any())
            {
                await _context.RequestClassifications.AddRangeAsync(classificationsToAdd, cancellationToken);
            }
        }

        // Handle draft status
        if (dto.SaveAsDraft)
        {
            request.RequestStatusId = StatusDraft;
        }
        // If not saving as draft and currently in PendingCompletion, keep existing status
        // Business rules may vary for status transitions

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
        // Extract active classifications (not soft-deleted)
        var activeClassifications = request.Classifications?
            .Where(rc => !rc.IsDeleted)
            .ToList() ?? new List<RequestClassification>();

        // Extract active related cases
        var activeRelatedCases = request.RelatedCases?
            .Where(rc => !rc.IsDeleted)
            .ToList() ?? new List<RelatedCase>();

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
            AttachmentsCount = request.Attachments?.Count(a => !a.IsDeleted) ?? 0,
            ClassificationIds = activeClassifications
                .Select(rc => rc.ClassificationId)
                .ToList(),
            RelatedCaseIds = activeRelatedCases
                .Select(rc => rc.Id)
                .ToList(),
            PrimaryMobile = request.PrimaryMobile,
            SecondaryMobile = request.SecondaryMobile,
            Email = request.Email
        };
    }
}
