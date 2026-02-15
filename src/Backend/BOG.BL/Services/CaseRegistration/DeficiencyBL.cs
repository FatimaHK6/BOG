using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for Deficiencies (نواقص الدعوى).
/// Handles deficiency retrieval and batch updates.
/// Enforces state validation (Draft or Deficiencies status only).
/// No automatic status changes or notifications when deficiencies are added/modified.
/// </summary>
public class DeficiencyBL : IDeficiencyBL
{
    private readonly IDeficiencyRepository _deficiencyRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeficiencyBL(
        IDeficiencyRepository deficiencyRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _deficiencyRepository = deficiencyRepository ?? throw new ArgumentNullException(nameof(deficiencyRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all deficiencies for a request.
    /// Includes DeficiencyDescription and DeficiencyType data for rich responses.
    /// </summary>
    public async Task<IEnumerable<DeficiencyVM>> GetDeficienciesAsync(int requestId, CancellationToken ct = default)
    {
        var deficiencies = await _deficiencyRepository.GetByRequestIdAsync(requestId, ct);
        return deficiencies.Select(d => MapToVM(d)).ToList();
    }

    /// <summary>
    /// Replaces all deficiencies for a request with the provided list.
    /// Soft deletes existing deficiencies and creates new ones.
    /// Validates request exists and is in editable state (Draft or Deficiencies).
    /// Note: Does NOT change request status or send notifications.
    /// </summary>
    public async Task<IEnumerable<DeficiencyVM>> UpdateDeficienciesAsync(int requestId, DeficienciesBatchUpdateDTO dto, CancellationToken ct = default)
    {
        // Validate request exists and is editable
        var request = await _requestRepository.GetByIdAsync(requestId, ct);
        if (request == null || request.IsDeleted)
            throw new KeyNotFoundException($"Case registration request {requestId} not found");

        // Check if request is in editable status (1=Draft, 6=PendingCompletion/Deficiencies)
        // Status 1 = Draft, Status 6 = PendingCompletion (deficiencies status)
        if (request.RequestStatusId != 1 && request.RequestStatusId != 6)
            throw new InvalidOperationException("Request cannot be edited in current status");

        // Soft delete existing deficiencies
        await _deficiencyRepository.DeleteByRequestIdAsync(requestId, ct);

        // Create new deficiencies from DTO
        var newDeficiencies = dto.Deficiencies
            .Select((d, index) => new RequestDeficiency
            {
                CaseRegistrationRequestId = requestId,
                DeficiencyDescriptionId = d.DeficiencyDescriptionId,
                DisplayOrder = index,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            })
            .ToList();

        foreach (var deficiency in newDeficiencies)
        {
            await _deficiencyRepository.AddAsync(deficiency, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Return saved deficiencies with joined data
        var savedDeficiencies = await _deficiencyRepository.GetByRequestIdAsync(requestId, ct);
        return savedDeficiencies.Select(d => MapToVM(d)).ToList();
    }

    /// <summary>
    /// Maps RequestDeficiency entity with eager-loaded relationships to DeficiencyVM.
    /// Assumes DeficiencyDescription and DeficiencyType are already loaded.
    /// </summary>
    private static DeficiencyVM MapToVM(RequestDeficiency entity)
    {
        return new DeficiencyVM
        {
            Id = entity.Id,
            CaseRegistrationRequestId = entity.CaseRegistrationRequestId,
            DeficiencyDescriptionId = entity.DeficiencyDescriptionId,
            DeficiencyTypeId = entity.DeficiencyDescription?.DeficiencyType?.Id ?? 0,
            DeficiencyTypeName = entity.DeficiencyDescription?.DeficiencyType?.Name ?? "",
            DeficiencyTypeNameAr = entity.DeficiencyDescription?.DeficiencyType?.NameAr ?? "",
            DescriptionAr = entity.DeficiencyDescription?.DescriptionAr ?? "",
            DescriptionEn = entity.DeficiencyDescription?.DescriptionEn,
            DisplayOrder = entity.DisplayOrder,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        };
    }
}
