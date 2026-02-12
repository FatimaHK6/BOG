using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for Related Cases (الدعاوى المرتبطة).
/// Handles related case retrieval and batch updates.
/// Enforces state validation (Draft or Deficiencies status only).
/// </summary>
public class RelatedCaseBL : IRelatedCaseBL
{
    private readonly IRelatedCaseRepository _relatedCaseRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RelatedCaseBL(
        IRelatedCaseRepository relatedCaseRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _relatedCaseRepository = relatedCaseRepository ?? throw new ArgumentNullException(nameof(relatedCaseRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all related cases for a request.
    /// </summary>
    public async Task<IEnumerable<RelatedCaseVM>> GetRelatedCasesAsync(int requestId, CancellationToken ct = default)
    {
        var relatedCases = await _relatedCaseRepository.GetByRequestIdAsync(requestId, ct);
        return relatedCases.Select(rc => new RelatedCaseVM
        {
            Id = rc.Id,
            CaseRegistrationRequestId = rc.CaseRegistrationRequestId,
            CourtId = rc.CourtId,
            CourtName = rc.Court?.NameAr, // Arabic court name for display
            CaseNumber = rc.CaseNumber,
            CaseYear = rc.CaseYear,
            CreatedDate = rc.CreatedDate,
            ModifiedDate = rc.ModifiedDate
        });
    }

    /// <summary>
    /// Replaces all related cases for a request with the provided list.
    /// Soft deletes existing related cases and creates new ones.
    /// Validates request exists and is in editable state (Draft or Deficiencies).
    /// </summary>
    public async Task<IEnumerable<RelatedCaseVM>> UpdateRelatedCasesAsync(int requestId, RelatedCasesBatchUpdateDTO dto, CancellationToken ct = default)
    {
        // Validate request exists and is editable
        var request = await _requestRepository.GetByIdAsync(requestId, ct);
        if (request == null || request.IsDeleted)
            throw new KeyNotFoundException($"Case registration request {requestId} not found");

        // Check if request is in editable status (1=Draft, 8=Deficiencies)
        if (request.RequestStatusId != 1 && request.RequestStatusId != 8)
            throw new InvalidOperationException("Request cannot be edited in current status");

        // Soft delete existing related cases
        await _relatedCaseRepository.DeleteByRequestIdAsync(requestId, ct);

        // Create new related cases from DTO
        var newRelatedCases = dto.RelatedCases.Select(rc => new RelatedCase
        {
            CaseRegistrationRequestId = requestId,
            CourtId = rc.CourtId,
            CaseNumber = rc.CaseNumber,
            CaseYear = rc.CaseYear,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToList();

        foreach (var relatedCase in newRelatedCases)
        {
            await _relatedCaseRepository.AddAsync(relatedCase, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Return saved related cases
        var savedRelatedCases = await _relatedCaseRepository.GetByRequestIdAsync(requestId, ct);
        return savedRelatedCases.Select(rc => new RelatedCaseVM
        {
            Id = rc.Id,
            CaseRegistrationRequestId = rc.CaseRegistrationRequestId,
            CourtId = rc.CourtId,
            CourtName = rc.Court?.NameAr,
            CaseNumber = rc.CaseNumber,
            CaseYear = rc.CaseYear,
            CreatedDate = rc.CreatedDate,
            ModifiedDate = rc.ModifiedDate
        });
    }
}
