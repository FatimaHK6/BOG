using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for Claims (طلبات الدعوى).
/// Handles claim retrieval and batch updates.
/// Enforces state validation (Draft or Deficiencies status only).
/// </summary>
public class ClaimBL : IClaimBL
{
    private readonly IClaimRepository _claimRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClaimBL(
        IClaimRepository claimRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all claims for a request.
    /// </summary>
    public async Task<IEnumerable<ClaimVM>> GetClaimsAsync(int requestId, CancellationToken ct = default)
    {
        var claims = await _claimRepository.GetByRequestIdAsync(requestId, ct);
        return claims.Select(c => new ClaimVM
        {
            Id = c.Id,
            CaseRegistrationRequestId = c.CaseRegistrationRequestId,
            ClaimText = c.ClaimText,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        });
    }

    /// <summary>
    /// Replaces all claims for a request with the provided list.
    /// Soft deletes existing claims and creates new ones.
    /// Validates request exists and is in editable state (Draft or Deficiencies).
    /// </summary>
    public async Task<IEnumerable<ClaimVM>> UpdateClaimsAsync(int requestId, ClaimsBatchUpdateDTO dto, CancellationToken ct = default)
    {
        // Validate request exists and is editable
        var request = await _requestRepository.GetByIdAsync(requestId, ct);
        if (request == null || request.IsDeleted)
            throw new KeyNotFoundException($"Case registration request {requestId} not found");

        // Check if request is in editable status (1=Draft, 6=PendingCompletion)
        if (request.RequestStatusId != 1 && request.RequestStatusId != 6)
            throw new InvalidOperationException("Request cannot be edited in current status");

        // Soft delete existing claims
        await _claimRepository.DeleteByRequestIdAsync(requestId, ct);

        // Create new claims from DTO
        var newClaims = dto.Claims.Select(c => new Claim
        {
            CaseRegistrationRequestId = requestId,
            ClaimText = c.ClaimText,
            DisplayOrder = 0, // Not used for ordering, kept for entity compatibility
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToList();

        foreach (var claim in newClaims)
        {
            await _claimRepository.AddAsync(claim, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Return saved claims
        var savedClaims = await _claimRepository.GetByRequestIdAsync(requestId, ct);
        return savedClaims.Select(c => new ClaimVM
        {
            Id = c.Id,
            CaseRegistrationRequestId = c.CaseRegistrationRequestId,
            ClaimText = c.ClaimText,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        });
    }
}
