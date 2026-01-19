using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Lookups;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using BOG.VM.Shared;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for case registration request lifecycle management.
/// Handles request creation, updates, submission validation, and state management.
/// Enforces all submission validation rules (ERR001-007).
/// </summary>
public class CaseRegistrationBL : ICaseRegistrationBL
{
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IRepository<AttachmentType> _attachmentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CaseRegistrationBL(
        ICaseRegistrationRequestRepository requestRepository,
        IRepository<AttachmentType> attachmentTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _attachmentTypeRepository = attachmentTypeRepository ?? throw new ArgumentNullException(nameof(attachmentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Creates a new case registration request in Draft state.
    /// </summary>
    public async Task<CaseRegistrationRequestVM> CreateRequestAsync(object requestData, CancellationToken cancellationToken = default)
    {
        if (requestData == null)
            throw new ArgumentNullException(nameof(requestData));

        // Handle both strongly-typed DTO and object types
        CaseRegistrationCreateDTO dto;
        if (requestData is CaseRegistrationCreateDTO typedDto)
        {
            dto = typedDto;
        }
        else if (requestData is IDictionary<string, object> dictData)
        {
            int courtId = Convert.ToInt32(dictData["courtId"]);
            string subject = dictData.ContainsKey("subject") ? dictData["subject"]?.ToString() ?? "" : "";
            string evidence = dictData.ContainsKey("evidence") ? dictData["evidence"]?.ToString() ?? "" : "";
            dto = new CaseRegistrationCreateDTO
            {
                CourtId = courtId,
                Subject = subject,
                Evidence = evidence
            };
        }
        else
        {
            throw new ArgumentException("Invalid request data format.", nameof(requestData));
        }

        if (dto.CourtId <= 0)
            throw new InvalidOperationException("Valid court ID is required.");

        // Create new request in Draft state (1)
        // Use a default user ID (9999 for tests, in production this should come from HttpContext)
        var request = new CaseRegistrationRequest
        {
            CourtId = dto.CourtId,
            Subject = dto.Subject,
            Evidence = dto.Evidence,
            RequestStatusId = 1, // Draft
            CreatedByUserId = 9999, // TODO: Get from HttpContext.User in production
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await _requestRepository.AddAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload request with navigation properties for mapping
        var savedRequest = await _requestRepository.GetByIdAsync(request.Id, cancellationToken);
        if (savedRequest == null)
            throw new InvalidOperationException($"Failed to retrieve created request with ID {request.Id}");

        return MapToViewModel(savedRequest);
    }

    /// <summary>
    /// Gets a case registration request by ID.
    /// </summary>
    public async Task<CaseRegistrationRequestVM?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);

        if (request == null || request.IsDeleted)
            return null;

        return MapToViewModel(request);
    }

    /// <summary>
    /// Gets case registration request with all related details.
    /// </summary>
    public async Task<CaseRegistrationRequestDetailsVM?> GetRequestWithDetailsAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);

        if (request == null || request.IsDeleted)
            return null;

        return MapToDetailedViewModel(request);
    }

    /// <summary>
    /// Updates a case registration request.
    /// Only allowed in Draft (1) and PendingCompletion (8) states.
    /// </summary>
    public async Task<CaseRegistrationRequestVM> UpdateRequestAsync(int requestId, object requestData, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        if (requestData == null)
            throw new ArgumentNullException(nameof(requestData));

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null || request.IsDeleted)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Can only update in Draft or PendingCompletion states
        if (request.RequestStatusId != 1 && request.RequestStatusId != 8)
            throw new InvalidOperationException("Can only update requests in Draft or PendingCompletion state.");

        // Handle both strongly-typed DTO and object types
        IDictionary<string, object> requestDict;
        if (requestData is CaseRegistrationUpdateDTO updateDto)
        {
            requestDict = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(updateDto.Subject))
                requestDict["subject"] = updateDto.Subject;
            if (!string.IsNullOrWhiteSpace(updateDto.Evidence))
                requestDict["evidence"] = updateDto.Evidence;
            if (updateDto.CourtId.HasValue && updateDto.CourtId > 0)
                requestDict["courtId"] = updateDto.CourtId;
        }
        else if (requestData is IDictionary<string, object> dictData)
        {
            requestDict = dictData;
        }
        else
        {
            throw new ArgumentException("Invalid request data format.", nameof(requestData));
        }

        // Update fields
        if (requestDict.ContainsKey("subject"))
            request.Subject = requestDict["subject"]?.ToString();

        if (requestDict.ContainsKey("evidence"))
            request.Evidence = requestDict["evidence"]?.ToString();

        if (requestDict.ContainsKey("courtId"))
            request.CourtId = Convert.ToInt32(requestDict["courtId"]);

        request.ModifiedDate = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToViewModel(request);
    }

    /// <summary>
    /// Submits a case registration request for review.
    /// Changes state from Draft (1) to New (3).
    /// Validates all business rules (ERR001-007).
    /// </summary>
    public async Task<CaseRegistrationRequestVM> SubmitRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        // Validate all submission rules
        await ValidateForSubmissionAsync(requestId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Change status from Draft (1) to New (3)
        request.RequestStatusId = 3;
        request.SubmissionDate = DateTime.UtcNow;
        request.ModifiedDate = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToViewModel(request);
    }

    /// <summary>
    /// Validates case registration request for submission.
    /// Checks ERR001-007 validation rules.
    /// </summary>
    public async Task<bool> ValidateForSubmissionAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        var errors = new List<string>();

        // ERR001: At least one plaintiff required
        if (!request.CaseRequestPlaintiffs?.Any() ?? true)
            errors.Add("ERR001: At least one plaintiff is required.");

        // ERR002: At least one defendant required
        if (!request.CaseRequestDefendants?.Any() ?? true)
            errors.Add("ERR002: At least one defendant is required.");

        // ERR006: Subject is required
        if (string.IsNullOrWhiteSpace(request.Subject))
            errors.Add("ERR006: Subject is required.");

        // ERR007: Evidence is required
        if (string.IsNullOrWhiteSpace(request.Evidence))
            errors.Add("ERR007: Evidence is required.");

        // ERR004: Applicant must be specified
        var hasApplicant = request.CaseRequestPlaintiffs?.Any(p => p.Plaintiff?.IsApplicant ?? false) ?? false;
        if (!hasApplicant)
            errors.Add("ERR004: At least one applicant must be specified.");

        // ERR005: Classifications must be specified
        if (!request.Classifications?.Any() ?? true)
            errors.Add("ERR005: At least one case classification is required.");

        // ERR003: Mandatory attachments must be complete
        var mandatoryTypes = await _attachmentTypeRepository.FindAsync(
            a => a.IsMandatory && !a.IsDeleted, cancellationToken);

        if (mandatoryTypes.Any())
        {
            var attachedTypeIds = request.Attachments?
                .Where(a => !a.IsDeleted)
                .Select(a => a.AttachmentTypeId)
                .ToHashSet() ?? new HashSet<int>();

            var missingTypes = mandatoryTypes.Where(mt => !attachedTypeIds.Contains(mt.Id)).ToList();
            if (missingTypes.Any())
            {
                var missingNames = string.Join(", ", missingTypes.Select(t => t.NameAr));
                errors.Add($"ERR003: Missing mandatory attachments: {missingNames}");
            }
        }

        if (errors.Any())
            throw new InvalidOperationException(string.Join(" | ", errors));

        return true;
    }

    /// <summary>
    /// Searches case registration requests with optional filters and pagination.
    /// </summary>
    public async Task<PagedResult<CaseRegistrationRequestVM>> SearchRequestsAsync(
        object searchCriteria,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1.", nameof(pageNumber));

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100.", nameof(pageSize));

        // TODO: Implement search with filters once search DTO is defined
        // For now, return all requests with pagination

        var allRequests = await _requestRepository.GetAllAsync(cancellationToken);
        var filteredRequests = allRequests.Where(r => !r.IsDeleted).ToList();

        var totalCount = filteredRequests.Count;
        var skipCount = (pageNumber - 1) * pageSize;
        var requests = filteredRequests
            .Skip(skipCount)
            .Take(pageSize)
            .Select(MapToViewModel)
            .ToList();

        return new PagedResult<CaseRegistrationRequestVM>(
            items: requests,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize
        );
    }

    /// <summary>
    /// Maps CaseRegistrationRequest entity to view model.
    /// </summary>
    private static CaseRegistrationRequestVM MapToViewModel(CaseRegistrationRequest request)
    {
        return new CaseRegistrationRequestVM
        {
            Id = request.Id,
            RequestStatusId = request.RequestStatusId,
            RequestStatusName = GetStatusName(request.RequestStatusId),
            CourtId = request.CourtId ?? 0,
            CourtName = request.Court?.NameAr ?? "",
            Subject = request.Subject,
            Evidence = request.Evidence,
            SubmissionDate = request.SubmissionDate,
            CompletionDeadline = request.CompletionDeadline,
            RejectionReason = request.RejectionReason,
            CaseNumber = request.CaseNumber,
            RegistrationNumber = request.RegistrationNumber,
            RegistrationDate = request.RegistrationDate,
            PlaintiffsCount = request.CaseRequestPlaintiffs?.Count ?? 0,
            DefendantsCount = request.CaseRequestDefendants?.Count ?? 0,
            AttachmentsCount = request.Attachments?.Where(a => !a.IsDeleted).Count() ?? 0,
            CreatedDate = request.CreatedDate,
            ModifiedDate = request.ModifiedDate
        };
    }

    /// <summary>
    /// Maps CaseRegistrationRequest with all details to view model.
    /// </summary>
    private static CaseRegistrationRequestDetailsVM MapToDetailedViewModel(CaseRegistrationRequest request)
    {
        return new CaseRegistrationRequestDetailsVM
        {
            Id = request.Id,
            RequestStatusId = request.RequestStatusId,
            RequestStatusName = GetStatusName(request.RequestStatusId),
            CourtId = request.CourtId ?? 0,
            CourtName = request.Court?.NameAr ?? "",
            Subject = request.Subject,
            Evidence = request.Evidence,
            SubmissionDate = request.SubmissionDate,
            CompletionDeadline = request.CompletionDeadline,
            RejectionReason = request.RejectionReason,
            CaseNumber = request.CaseNumber,
            RegistrationNumber = request.RegistrationNumber,
            RegistrationDate = request.RegistrationDate,
            // Note: Full details (plaintiffs, defendants, etc.) would be populated here
            // For now, these are left empty as the relationships need to be loaded separately
            CreatedDate = request.CreatedDate,
            ModifiedDate = request.ModifiedDate
        };
    }

    /// <summary>
    /// Gets status name from status ID.
    /// </summary>
    private static string GetStatusName(int statusId) => statusId switch
    {
        1 => "Draft",
        3 => "New",
        5 => "OnJudgeDesk",
        6 => "Registered",
        8 => "PendingCompletion",
        9 => "UnderReview",
        10 => "Rejected",
        _ => "Unknown"
    };
}
