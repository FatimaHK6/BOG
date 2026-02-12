using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DTO.AdditionalInfo;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.VM.AdditionalInfo;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for additional information management in case registration.
/// Handles three types of additional information:
/// 1. Management Decision Cancellation
/// 2. Service/Retirement Rights
/// 3. Trademark Disputes
/// </summary>
public class AdditionalInfoBL : IAdditionalInfoBL
{
    private readonly IAdditionalInfoRepository _additionalInfoRepository;
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdditionalInfoBL(
        IAdditionalInfoRepository additionalInfoRepository,
        ICaseRegistrationRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _additionalInfoRepository = additionalInfoRepository ?? throw new ArgumentNullException(nameof(additionalInfoRepository));
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Adds or updates additional information for a case registration request.
    /// Handles all three types: Management Decision, Service Rights, and Trademark.
    /// All types are optional - if no data provided, returns empty VM.
    /// </summary>
    public async Task<AdditionalInfoVM> AddOrUpdateAsync(int requestId, object additionalInfoData, CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        if (additionalInfoData == null)
            throw new ArgumentNullException(nameof(additionalInfoData));

        // Convert to DTO
        AdditionalInfoDTO infoDto = ConvertToDTO(additionalInfoData);

        // Get request and validate it exists
        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null || request.IsDeleted)
            throw new InvalidOperationException($"Request {requestId} not found or has been deleted.");

        // Check if additional info already exists
        // Use tracked query for updates to ensure nested navigation properties are tracked
        var existingInfo = await _additionalInfoRepository.GetByRequestIdAsyncForUpdateAsync(requestId, cancellationToken);

        AdditionalInfo infoEntity;

        if (existingInfo != null)
        {
            // Update existing record
            infoEntity = existingInfo;
            infoEntity.ModifiedDate = DateTime.UtcNow;

            // Handle type-specific updates
            UpdateTypeSpecificData(infoEntity, infoDto);
            await _additionalInfoRepository.UpdateAsync(infoEntity, cancellationToken);
        }
        else
        {
            // Create new record
            infoEntity = new AdditionalInfo
            {
                CaseRegistrationRequestId = requestId,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // Handle type-specific data for new record
            UpdateTypeSpecificData(infoEntity, infoDto);
            await _additionalInfoRepository.AddAsync(infoEntity, cancellationToken);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with all relationships
        var savedInfo = await _additionalInfoRepository.GetByRequestIdAsync(requestId, cancellationToken);
        if (savedInfo == null)
            throw new InvalidOperationException($"Failed to retrieve additional info for request {requestId}");

        return MapToViewModel(savedInfo);
    }

    /// <summary>
    /// Gets additional information for a case registration request.
    /// </summary>
    public async Task<AdditionalInfoVM?> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var additionalInfo = await _additionalInfoRepository.GetByRequestIdAsync(requestId, cancellationToken);

        if (additionalInfo == null || additionalInfo.IsDeleted)
            return null;

        return MapToViewModel(additionalInfo);
    }

    /// <summary>
    /// Gets additional information by ID.
    /// </summary>
    public async Task<AdditionalInfoVM?> GetByIdAsync(int additionalInfoId, CancellationToken cancellationToken = default)
    {
        if (additionalInfoId <= 0)
            throw new ArgumentException("Invalid additional info ID.", nameof(additionalInfoId));

        var additionalInfo = await _additionalInfoRepository.GetByIdAsync(additionalInfoId, cancellationToken);

        if (additionalInfo == null || additionalInfo.IsDeleted)
            return null;

        return MapToViewModel(additionalInfo);
    }

    /// <summary>
    /// Deletes additional information for a case registration request (soft delete).
    /// </summary>
    public async Task DeleteByRequestIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var additionalInfo = await _additionalInfoRepository.GetByRequestIdAsync(requestId, cancellationToken);
        if (additionalInfo == null || additionalInfo.IsDeleted)
            throw new InvalidOperationException($"Additional info for request {requestId} not found.");

        // Soft delete
        additionalInfo.IsDeleted = true;
        additionalInfo.ModifiedDate = DateTime.UtcNow;

        await _additionalInfoRepository.UpdateAsync(additionalInfo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Soft-deletes additional information by ID.
    /// </summary>
    public async Task DeleteAsync(int additionalInfoId, CancellationToken cancellationToken = default)
    {
        if (additionalInfoId <= 0)
            throw new ArgumentException("Invalid additional info ID.", nameof(additionalInfoId));

        var additionalInfo = await _additionalInfoRepository.GetByIdAsync(additionalInfoId, cancellationToken);
        if (additionalInfo == null || additionalInfo.IsDeleted)
            throw new InvalidOperationException($"Additional info with ID {additionalInfoId} not found.");

        // Soft delete
        additionalInfo.IsDeleted = true;
        additionalInfo.ModifiedDate = DateTime.UtcNow;

        await _additionalInfoRepository.UpdateAsync(additionalInfo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Converts object data to AdditionalInfoDTO.
    /// </summary>
    private static AdditionalInfoDTO ConvertToDTO(object data)
    {
        if (data is AdditionalInfoDTO dto)
            return dto;

        if (data is IDictionary<string, object> dict)
        {
            return new AdditionalInfoDTO
            {
                // Type 1
                DecisionNumber = dict.TryGetValue("decisionNumber", out var dn) ? dn?.ToString() : null,
                DecisionDate = dict.TryGetValue("decisionDate", out var dd) && DateTime.TryParse(dd?.ToString() ?? "", out var ddResult) ? ddResult : null,
                NotificationDate = dict.TryGetValue("notificationDate", out var nd) && DateTime.TryParse(nd?.ToString() ?? "", out var ndResult) ? ndResult : null,
                NotificationMethodId = dict.TryGetValue("notificationMethodId", out var nm) && int.TryParse(nm?.ToString() ?? "", out var nmResult) ? nmResult : null,
                IssuingAuthorityId = dict.TryGetValue("issuingAuthorityId", out var ia) && int.TryParse(ia?.ToString() ?? "", out var iaResult) ? iaResult : null,

                // Type 2
                HasComplaint = dict.TryGetValue("hasComplaint", out var hc) && bool.TryParse(hc?.ToString() ?? "", out var hcResult) ? hcResult : null,
                ComplaintNumber = dict.TryGetValue("complaintNumber", out var cn) ? cn?.ToString() : null,
                ComplaintDate = dict.TryGetValue("complaintDate", out var cd) && DateTime.TryParse(cd?.ToString() ?? "", out var cdResult) ? cdResult : null,
                ComplaintAuthorityId = dict.TryGetValue("complaintAuthorityId", out var ca) && int.TryParse(ca?.ToString() ?? "", out var caResult) ? caResult : null,
                ComplaintDecisionDate = dict.TryGetValue("complaintDecisionDate", out var cdd) && DateTime.TryParse(cdd?.ToString() ?? "", out var cddResult) ? cddResult : null,
                SystemResult = dict.TryGetValue("systemResult", out var sr) ? sr?.ToString() : null,

                // Type 3
                RequestNumber = dict.TryGetValue("requestNumber", out var rn) ? rn?.ToString() : null,
                RequestDate = dict.TryGetValue("requestDate", out var rd) && DateTime.TryParse(rd?.ToString() ?? "", out var rdResult) ? rdResult : null
            };
        }

        throw new ArgumentException("Invalid additional info data format.", nameof(data));
    }

    /// <summary>
    /// Updates type-specific data for all three types.
    /// Detects which types have data and saves/deletes accordingly.
    /// </summary>
    private void UpdateTypeSpecificData(AdditionalInfo additionalInfo, AdditionalInfoDTO dto)
    {
        // Type 1: Management Decision
        bool hasType1Data = !string.IsNullOrEmpty(dto.DecisionNumber) ||
                           dto.DecisionDate.HasValue ||
                           dto.NotificationDate.HasValue ||
                           dto.NotificationMethodId.HasValue ||
                           dto.IssuingAuthorityId.HasValue;

        // Type 2: Service/Retirement Rights
        bool hasType2Data = dto.HasComplaint.HasValue ||
                           !string.IsNullOrEmpty(dto.ComplaintNumber) ||
                           dto.ComplaintDate.HasValue ||
                           dto.ComplaintAuthorityId.HasValue ||
                           dto.ComplaintDecisionDate.HasValue ||
                           !string.IsNullOrEmpty(dto.SystemResult);

        // Type 3: Trademark
        bool hasType3Data = !string.IsNullOrEmpty(dto.RequestNumber) ||
                           dto.RequestDate.HasValue;

        // If no data in any type, just keep base record
        if (!hasType1Data && !hasType2Data && !hasType3Data)
            return;

        // Handle Type 1
        if (hasType1Data)
        {
            if (additionalInfo.ManagementDecision != null)
            {
                // Update existing
                additionalInfo.ManagementDecision.DecisionNumber = dto.DecisionNumber ?? "";
                additionalInfo.ManagementDecision.DecisionDate = dto.DecisionDate ?? DateTime.UtcNow;
                additionalInfo.ManagementDecision.NotificationDate = dto.NotificationDate ?? DateTime.UtcNow;
                additionalInfo.ManagementDecision.NotificationMethodId = dto.NotificationMethodId;
                additionalInfo.ManagementDecision.IssuingAuthorityId = dto.IssuingAuthorityId;
                additionalInfo.ManagementDecision.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                // Create new
                additionalInfo.ManagementDecision = new AdditionalInfoManagementDecision
                {
                    DecisionNumber = dto.DecisionNumber ?? "",
                    DecisionDate = dto.DecisionDate ?? DateTime.UtcNow,
                    NotificationDate = dto.NotificationDate ?? DateTime.UtcNow,
                    NotificationMethodId = dto.NotificationMethodId,
                    IssuingAuthorityId = dto.IssuingAuthorityId,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
            }
        }
        else
        {
            // Remove Type 1 if it exists
            additionalInfo.ManagementDecision = null;
        }

        // Handle Type 2
        if (hasType2Data)
        {
            if (additionalInfo.ServiceRights != null)
            {
                // Update existing
                additionalInfo.ServiceRights.HasComplaint = dto.HasComplaint;
                additionalInfo.ServiceRights.ComplaintNumber = dto.ComplaintNumber;
                additionalInfo.ServiceRights.ComplaintDate = dto.ComplaintDate;
                additionalInfo.ServiceRights.ComplaintAuthorityId = dto.ComplaintAuthorityId;
                additionalInfo.ServiceRights.ComplaintDecisionDate = dto.ComplaintDecisionDate;
                additionalInfo.ServiceRights.SystemResult = dto.SystemResult;
                additionalInfo.ServiceRights.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                // Create new
                additionalInfo.ServiceRights = new AdditionalInfoServiceRights
                {
                    HasComplaint = dto.HasComplaint,
                    ComplaintNumber = dto.ComplaintNumber,
                    ComplaintDate = dto.ComplaintDate,
                    ComplaintAuthorityId = dto.ComplaintAuthorityId,
                    ComplaintDecisionDate = dto.ComplaintDecisionDate,
                    SystemResult = dto.SystemResult,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
            }
        }
        else
        {
            // Remove Type 2 if it exists
            additionalInfo.ServiceRights = null;
        }

        // Handle Type 3
        if (hasType3Data)
        {
            if (additionalInfo.Trademark != null)
            {
                // Update existing
                additionalInfo.Trademark.RequestNumber = dto.RequestNumber ?? "";
                additionalInfo.Trademark.RequestDate = dto.RequestDate ?? DateTime.UtcNow;
                additionalInfo.Trademark.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                // Create new
                additionalInfo.Trademark = new AdditionalInfoTrademark
                {
                    RequestNumber = dto.RequestNumber ?? "",
                    RequestDate = dto.RequestDate ?? DateTime.UtcNow,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
            }
        }
        else
        {
            // Remove Type 3 if it exists
            additionalInfo.Trademark = null;
        }

        // Note: The repository's UpdateAsync should handle navigation properties properly
        // if the DbContext is configured correctly
    }

    /// <summary>
    /// Maps AdditionalInfo entity to view model.
    /// Extracts and displays only the type-specific fields that have data.
    /// </summary>
    private static AdditionalInfoVM MapToViewModel(AdditionalInfo additionalInfo)
    {
        if (additionalInfo == null)
            throw new ArgumentNullException(nameof(additionalInfo));

        return new AdditionalInfoVM
        {
            Id = additionalInfo.Id,
            CaseRegistrationRequestId = additionalInfo.CaseRegistrationRequestId,

            // Type 1
            DecisionNumber = additionalInfo.ManagementDecision?.DecisionNumber,
            DecisionDate = additionalInfo.ManagementDecision?.DecisionDate,
            NotificationDate = additionalInfo.ManagementDecision?.NotificationDate,
            NotificationMethod = additionalInfo.ManagementDecision?.NotificationMethod?.NameAr,
            NotificationMethodId = additionalInfo.ManagementDecision?.NotificationMethodId,
            IssuingAuthority = additionalInfo.ManagementDecision?.IssuingAuthority?.NameAr,
            IssuingAuthorityId = additionalInfo.ManagementDecision?.IssuingAuthorityId,

            // Type 2
            HasComplaint = additionalInfo.ServiceRights?.HasComplaint,
            ComplaintNumber = additionalInfo.ServiceRights?.ComplaintNumber,
            ComplaintDate = additionalInfo.ServiceRights?.ComplaintDate,
            ComplaintAuthority = additionalInfo.ServiceRights?.ComplaintAuthority?.NameAr,
            ComplaintAuthorityId = additionalInfo.ServiceRights?.ComplaintAuthorityId,
            ComplaintDecisionDate = additionalInfo.ServiceRights?.ComplaintDecisionDate,
            SystemResult = additionalInfo.ServiceRights?.SystemResult,

            // Type 3
            RequestNumber = additionalInfo.Trademark?.RequestNumber,
            RequestDate = additionalInfo.Trademark?.RequestDate,

            CreatedDate = additionalInfo.CreatedDate,
            ModifiedDate = additionalInfo.ModifiedDate
        };
    }
}
