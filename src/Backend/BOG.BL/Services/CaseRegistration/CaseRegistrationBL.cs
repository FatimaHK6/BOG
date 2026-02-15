using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Lookups;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using BOG.VM.Shared;
using Microsoft.EntityFrameworkCore;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for case registration request lifecycle management.
/// Handles request creation, updates, submission validation, and state management.
/// Enforces all submission validation rules (ERR002-007).
/// </summary>
public class CaseRegistrationBL : ICaseRegistrationBL
{
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly IRepository<AttachmentType> _attachmentTypeRepository;
    private readonly IRepository<Classification> _classificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public CaseRegistrationBL(
        ICaseRegistrationRequestRepository requestRepository,
        IRepository<AttachmentType> attachmentTypeRepository,
        IRepository<Classification> classificationRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _attachmentTypeRepository = attachmentTypeRepository ?? throw new ArgumentNullException(nameof(attachmentTypeRepository));
        _classificationRepository = classificationRepository ?? throw new ArgumentNullException(nameof(classificationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _context = context ?? throw new ArgumentNullException(nameof(context));
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
            throw new InvalidOperationException("معرف المحكمة مطلوب وصحيح");

        // Create new request in Draft state (1)
        // Use a default user ID (1 for tests, in production this should come from HttpContext)
        var request = new CaseRegistrationRequest
        {
            CourtId = dto.CourtId,
            Subject = dto.Subject,
            Evidence = dto.Evidence,
            RequestStatusId = 1, // Draft
            CaseTypeId = 1, // Default to Administrative (إداري) - user changes via edit page
            CreatedByUserId = 1, // TODO: Get from HttpContext.User in production
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await _requestRepository.AddAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload request with navigation properties for mapping
        var savedRequest = await _requestRepository.GetByIdAsync(request.Id, cancellationToken);
        if (savedRequest == null)
            throw new InvalidOperationException($"فشل في استرجاع الطلب المنشأ برقم {request.Id}");

        return MapToViewModel(savedRequest);
    }

    /// <summary>
    /// Gets a case registration request by ID.
    /// </summary>
    public async Task<CaseRegistrationRequestVM?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        // Clear change tracker to ensure fresh load from database
        _unitOfWork.ClearChangeTracker();

        var request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);

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

        // Load with all details including Classifications so EF can track changes
        var request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);
        if (request == null || request.IsDeleted)
            throw new InvalidOperationException($"الطلب {requestId} غير موجود");

        // Can only update in Draft or PendingCompletion states
        if (request.RequestStatusId != 1 && request.RequestStatusId != 8)
            throw new InvalidOperationException("يمكن تحديث الطلبات فقط في حالة المسودة أو في الانتظار للاستكمال");

        // Handle both strongly-typed DTO and object types
        IDictionary<string, object> requestDict;
        if (requestData is CaseRegistrationUpdateDTO updateDto)
        {
            requestDict = new Dictionary<string, object>();
            // Always add fields to dictionary - let the update logic handle null/empty values
            requestDict["subject"] = updateDto.Subject;
            requestDict["evidence"] = updateDto.Evidence;
            requestDict["courtId"] = updateDto.CourtId;
            requestDict["caseTypeId"] = updateDto.CaseTypeId;
            requestDict["notes"] = updateDto.Notes;
            requestDict["classificationIds"] = updateDto.ClassificationIds ?? new List<int>();
            // Add contact information fields
            requestDict["primaryMobile"] = updateDto.PrimaryMobile;
            requestDict["secondaryMobile"] = updateDto.SecondaryMobile;
            requestDict["email"] = updateDto.Email;
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
        // Subject - validate length if provided
        if (requestDict.ContainsKey("subject"))
        {
            var subject = requestDict["subject"]?.ToString();
            if (!string.IsNullOrWhiteSpace(subject))
            {
                if (subject.Length > 4000)
                    throw new ArgumentException("الموضوع لا يمكن أن يتجاوز 4000 حرف");
                request.Subject = subject;
            }
            // If empty, keep existing value (don't clear required field)
        }

        // Evidence - validate length if provided
        if (requestDict.ContainsKey("evidence"))
        {
            var evidence = requestDict["evidence"]?.ToString();
            if (!string.IsNullOrWhiteSpace(evidence))
            {
                if (evidence.Length > 4000)
                    throw new ArgumentException("الأدلة لا يمكن أن تتجاوز 4000 حرف");
                request.Evidence = evidence;
            }
            // If empty, keep existing value (don't clear required field)
        }

        if (requestDict.ContainsKey("courtId"))
        {
            if (requestDict["courtId"] != null && int.TryParse(requestDict["courtId"]?.ToString(), out int courtId) && courtId > 0)
                request.CourtId = courtId;
        }

        // CaseTypeId - validate if provided
        if (requestDict.ContainsKey("caseTypeId"))
        {
            if (requestDict["caseTypeId"] != null && int.TryParse(requestDict["caseTypeId"]?.ToString(), out int caseTypeId) && caseTypeId > 0)
            {
                if (caseTypeId < 1 || caseTypeId > 2)
                    throw new ArgumentException("نوع الدعوى غير صحيح (يجب أن يكون 1 أو 2)");
                request.CaseTypeId = caseTypeId;
            }
        }

        // Notes - optional field
        if (requestDict.ContainsKey("notes"))
        {
            var notes = requestDict["notes"]?.ToString();
            if (!string.IsNullOrWhiteSpace(notes))
            {
                if (notes.Length > 4000)
                    throw new ArgumentException("الملاحظات لا يمكن أن تتجاوز 4000 حرف");
                request.Notes = notes;
            }
            else
            {
                request.Notes = null; // Allow clearing optional field
            }
        }

        // Handle classifications update with differential logic
        List<int> newClassificationIds = new List<int>();
        if (requestDict.ContainsKey("classificationIds"))
        {
            var classificationIds = requestDict["classificationIds"] as System.Collections.IEnumerable;
            if (classificationIds != null)
            {
                var idList = new List<int>();
                foreach (var classId in classificationIds)
                {
                    if (int.TryParse(classId?.ToString(), out int id) && id > 0)
                    {
                        idList.Add(id);
                    }
                }

                // VALIDATE: Ensure all classification IDs exist and are active
                var validClassifications = await _classificationRepository.FindAsync(
                    c => idList.Contains(c.Id) && c.IsActive && !c.IsDeleted,
                    cancellationToken);

                var validIds = validClassifications.Select(c => c.Id).ToHashSet();
                var invalidIds = idList.Where(id => !validIds.Contains(id)).ToList();

                if (invalidIds.Any())
                {
                    throw new InvalidOperationException(
                        $"معرفات التصنيف غير صالحة: {string.Join(", ", invalidIds)}");
                }

                newClassificationIds = idList.Distinct().ToList();
            }
        }

        // Apply differential update to classifications
        // Load existing classifications (only non-deleted)
        var existingClassifications = await _context.RequestClassifications
            .Where(rc => rc.CaseRegistrationRequestId == requestId && !rc.IsDeleted)
            .ToListAsync(cancellationToken);

        var existingClassificationIds = existingClassifications
            .Select(rc => rc.ClassificationId)
            .ToHashSet();

        var newClassificationIdSet = newClassificationIds.ToHashSet();

        // Remove classifications that are no longer selected
        var classificationsToRemove = existingClassifications
            .Where(rc => !newClassificationIdSet.Contains(rc.ClassificationId))
            .ToList();

        if (classificationsToRemove.Any())
        {
            _context.RequestClassifications.RemoveRange(classificationsToRemove);
        }

        // Add new classifications that don't already exist
        var classificationsToAdd = newClassificationIdSet
            .Where(cid => !existingClassificationIds.Contains(cid))
            .Select(cid => new RequestClassification
            {
                CaseRegistrationRequestId = requestId,
                ClassificationId = cid,
                DisplayOrder = 0
            })
            .ToList();

        if (classificationsToAdd.Any())
        {
            await _context.RequestClassifications.AddRangeAsync(classificationsToAdd, cancellationToken);
        }

        // Update contact information if provided
        // Primary Mobile - validate format if provided
        if (requestDict.ContainsKey("primaryMobile"))
        {
            var mobile = requestDict["primaryMobile"]?.ToString();
            if (!string.IsNullOrWhiteSpace(mobile))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^05\d{8}$"))
                    throw new ArgumentException("رقم الجوال الأساسي يجب أن يكون 10 أرقام ويبدأ بـ 05");
                request.PrimaryMobile = mobile;
            }
            else
            {
                request.PrimaryMobile = null; // Allow clearing optional field
            }
        }

        // Secondary Mobile - validate format if provided
        if (requestDict.ContainsKey("secondaryMobile"))
        {
            var mobile = requestDict["secondaryMobile"]?.ToString();
            if (!string.IsNullOrWhiteSpace(mobile))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^05\d{8}$"))
                    throw new ArgumentException("رقم الجوال الثانوي يجب أن يكون 10 أرقام ويبدأ بـ 05");
                request.SecondaryMobile = mobile;
            }
            else
            {
                request.SecondaryMobile = null; // Allow clearing optional field
            }
        }

        // Email - validate format if provided
        if (requestDict.ContainsKey("email"))
        {
            var email = requestDict["email"]?.ToString();
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (email.Length > 255)
                    throw new ArgumentException("البريد الإلكتروني لا يمكن أن يتجاوز 255 حرف");
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                    throw new ArgumentException("صيغة البريد الإلكتروني غير صحيحة");
                request.Email = email;
            }
            else
            {
                request.Email = null; // Allow clearing optional field
            }
        }

        request.ModifiedDate = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // CRITICAL: Clear change tracker to ensure fresh load from database
        // Without this, EF Core might return cached entities that don't include
        // the newly added classifications due to tracking state issues
        _unitOfWork.ClearChangeTracker();

        // Reload with all details including classifications
        var updatedRequest = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);
        if (updatedRequest == null)
            throw new InvalidOperationException($"Failed to reload request {requestId}");

        var result = MapToViewModel(updatedRequest);

        return result;
    }

    /// <summary>
    /// Submits a case registration request for review.
    /// Changes state from Draft (1) to New (3).
    /// Validates all business rules (ERR002-007).
    /// </summary>
    public async Task<CaseRegistrationRequestVM> SubmitRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        // Validate all submission rules
        await ValidateForSubmissionAsync(requestId, cancellationToken);

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"الطلب {requestId} غير موجود");

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
    /// Checks ERR002-007 validation rules.
    /// </summary>
    public async Task<bool> ValidateForSubmissionAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"الطلب {requestId} غير موجود");

        var errors = new List<string>();

        // ERR002: At least one defendant required
        if (!request.CaseRequestDefendants?.Any() ?? true)
            errors.Add("يجب تحديد مدعى عليه واحد على الأقل");

        // ERR006: Subject is required
        if (string.IsNullOrWhiteSpace(request.Subject))
            errors.Add("الموضوع مطلوب");

        // ERR007: Evidence is required
        if (string.IsNullOrWhiteSpace(request.Evidence))
            errors.Add("الأدلة مطلوبة");

        // ERR004: If plaintiffs exist, at least one must be applicant
        if (request.CaseRequestPlaintiffs?.Any() ?? false)
        {
            var hasApplicant = request.CaseRequestPlaintiffs.Any(p => p.Plaintiff?.IsApplicant ?? false);
            if (!hasApplicant)
                errors.Add("يجب تحديد مدعٍ واحد على الأقل كمدعٍ");
        }

        // ERR005: Classifications must be specified
        if (!request.Classifications?.Any() ?? true)
            errors.Add("يجب تحديد تصنيف واحد على الأقل للدعوى");

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
                errors.Add($"المرفقات الإلزامية المفقودة: {missingNames}");
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
        // Debug: Log classifications info
        var allClassifications = request.Classifications ?? new List<RequestClassification>();
        var activeClassifications = allClassifications.Where(rc => !rc.IsDeleted).ToList();

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
            ClassificationIds = activeClassifications
                .Select(rc => rc.ClassificationId)
                .ToList(),
            PrimaryMobile = request.PrimaryMobile,
            SecondaryMobile = request.SecondaryMobile,
            Email = request.Email,
            RelatedCases = request.RelatedCases?
                .Where(rc => !rc.IsDeleted)
                .Select(rc => new RelatedCaseVM
                {
                    Id = rc.Id,
                    CaseRegistrationRequestId = rc.CaseRegistrationRequestId,
                    CourtId = rc.CourtId,
                    CourtName = rc.Court?.NameAr,
                    CaseNumber = rc.CaseNumber,
                    CaseYear = rc.CaseYear,
                    CreatedDate = rc.CreatedDate,
                    ModifiedDate = rc.ModifiedDate
                })
                .ToList() ?? new List<RelatedCaseVM>(),
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
            Classifications = request.Classifications?
                .Where(rc => !rc.IsDeleted)
                .OrderBy(rc => rc.DisplayOrder)
                .Select(rc => new CaseClassificationVM
                {
                    Id = rc.Classification.Id,
                    NameAr = rc.Classification.NameAr,
                    NameEn = rc.Classification.Name,
                    Code = rc.Classification.Description,
                    Level1 = rc.Classification.Level1 ?? string.Empty,
                    Level2 = rc.Classification.Level2 ?? string.Empty,
                    Level3 = rc.Classification.Level3 ?? string.Empty,
                    Level4 = rc.Classification.Level4 ?? string.Empty
                })
                .ToList() ?? new List<CaseClassificationVM>(),
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
