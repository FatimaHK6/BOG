using BOG.BL.Interfaces.CaseRegistration;
using BOG.DAL.Interfaces;
using BOG.DbModel.Constants;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Lookups;
using BOG.DTO.CaseRegistration;
using BOG.Integration.DTOs.CaseManagement;
using BOG.Integration.DTOs.Sms;
using BOG.Integration.DTOs.Email;
using BOG.Integration.Enums;
using BOG.Integration.Interfaces;
using Microsoft.Extensions.Logging;

namespace BOG.BL.Services.CaseRegistration;

/// <summary>
/// Business logic service for case registration request state transitions and actions.
/// Handles Register, Reject, SendToJudge, RequestCompletion actions and auto-rejection.
/// Manages state machine transitions and sends notifications via SMS and Email.
/// </summary>
public class RequestActionBL : IRequestActionBL
{
    private readonly ICaseRegistrationRequestRepository _requestRepository;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ICaseManagementService _caseManagementService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestActionBL> _logger;

    public RequestActionBL(
        ICaseRegistrationRequestRepository requestRepository,
        ISmsService smsService,
        IEmailService emailService,
        ICaseManagementService caseManagementService,
        IUnitOfWork unitOfWork,
        ILogger<RequestActionBL> logger)
    {
        _requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _caseManagementService = caseManagementService ?? throw new ArgumentNullException(nameof(caseManagementService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Takes an action on a case registration request.
    /// </summary>
    public async Task<object> TakeActionAsync(int requestId, object actionData, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        if (actionData == null)
            throw new ArgumentNullException(nameof(actionData));

        // Handle both strongly-typed DTO and object types
        string action;
        string? notes;
        object? deficiencies;

        if (actionData is TakeActionDTO typedDto)
        {
            action = typedDto.Action;
            notes = typedDto.Notes;
            deficiencies = typedDto.Deficiencies;
        }
        else if (actionData is IDictionary<string, object> dictData)
        {
            action = dictData["action"]?.ToString() ?? "";
            notes = dictData.ContainsKey("notes") ? dictData["notes"]?.ToString() : null;
            deficiencies = dictData.ContainsKey("deficiencies") ? dictData["deficiencies"] : null;
        }
        else
        {
            throw new ArgumentException("Invalid action data format.", nameof(actionData));
        }

        var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Validate state transition
        await ValidateStateTransitionAsync(request.RequestStatusId, action);

        return action switch
        {
            "Register" => await RegisterCaseAsync(requestId, cancellationToken),
            "SendToJudge" => await SendToJudgeDeskAsync(requestId, cancellationToken),
            "Reject" => await RejectRequestAsync(requestId, notes ?? "No reason provided", cancellationToken),
            "RequestCompletion" => await RequestCompletionAsync(requestId, deficiencies, cancellationToken),
            "Complete" => await CompleteCompletionAsync(requestId, cancellationToken),
            _ => throw new InvalidOperationException($"Invalid action: {action}")
        };
    }

    /// <summary>
    /// Registers a case with the external case management system.
    /// </summary>
    public async Task<object> RegisterCaseAsync(int requestId, CancellationToken cancellationToken = default, RequestDecisionDTO? decision = null)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetWithDetailsForUpdateAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Preserve CaseTypeId from decision if provided (for CompleteRequestAsync flow)
        if (decision != null && decision.CaseTypeId > 0)
            request.CaseTypeId = decision.CaseTypeId;

        // Validate status is editable (Draft, New, or PendingCompletion)
        if (request.RequestStatusId != RequestStatusIds.Draft &&
            request.RequestStatusId != RequestStatusIds.New &&
            request.RequestStatusId != RequestStatusIds.PendingCompletion)
            throw new InvalidOperationException("Request not in valid status for registration. Only Draft, New, or PendingCompletion requests can be registered.");

        try
        {
            // Prepare case data for external system
            var caseData = new CaseRegistrationData
            {
                RequestId = requestId,
                CourtId = request.CourtId,
                CourtName = request.Court?.NameAr ?? GetCourtNameById(request.CourtId ?? 1),
                Subject = request.Subject,
                Evidence = request.Evidence,
                Plaintiffs = request.CaseRequestPlaintiffs?
                    .Select(p => new CasePartyData
                    {
                        FullName = GetPlaintiffFullName(p.Plaintiff),
                        IdentityNumber = p.Plaintiff?.IdentityNumber,
                        Address = GetPlaintiffAddress(p.Plaintiff),
                        PhoneNumber = p.Plaintiff?.MobileNumber,
                        Email = p.Plaintiff?.Email
                    }).ToList() ?? new List<CasePartyData>(),
                Defendants = request.CaseRequestDefendants?
                    .Select(d => new CasePartyData
                    {
                        FullName = d.Defendant?.FullName ?? "",
                        IdentityNumber = d.Defendant?.IdentityNumber,
                        Address = d.Defendant?.AddressText
                    }).ToList() ?? new List<CasePartyData>()
            };

            // Call external case management service
            var result = await _caseManagementService.RegisterCaseAsync(caseData, cancellationToken);

            if (!result.IsSuccess)
                throw new InvalidOperationException($"Case registration failed: {result.ErrorMessage}");

            // Update request with case details
            request.CaseNumber = result.CaseNumber;
            request.RegistrationNumber = result.RegistrationNumber;
            request.RegistrationDate = result.RegistrationDate;
            request.RequestStatusId = RequestStatusIds.Registered;
            request.ModifiedDate = DateTime.UtcNow;

            await _requestRepository.UpdateAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Case {CaseNumber} registered for request {RequestId}", result.CaseNumber, requestId);

            // Send notifications (fire and forget)
            _ = SendRegistrationNotificationsAsync(request, result, cancellationToken);

            return MapToViewModel(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering case for request {RequestId}", requestId);
            throw;
        }
    }

    /// <summary>
    /// Rejects a case registration request.
    /// </summary>
    public async Task<object> RejectRequestAsync(int requestId, string rejectionReason, CancellationToken cancellationToken = default, RequestDecisionDTO? decision = null)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required.", nameof(rejectionReason));

        var request = await _requestRepository.GetWithDetailsForUpdateAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Preserve CaseTypeId from decision if provided (for CompleteRequestAsync flow)
        if (decision != null && decision.CaseTypeId > 0)
            request.CaseTypeId = decision.CaseTypeId;

        request.RequestStatusId = RequestStatusIds.Rejected;
        request.RejectionReason = rejectionReason;
        request.ModifiedDate = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Request {RequestId} rejected with reason: {Reason}", requestId, rejectionReason);

        // Send rejection notifications
        _ = SendRejectionNotificationsAsync(request, cancellationToken);

        return MapToViewModel(request);
    }

    /// <summary>
    /// Requests completion of missing documents.
    /// </summary>
    public async Task<object> RequestCompletionAsync(int requestId, object? deficiencies, CancellationToken cancellationToken = default, RequestDecisionDTO? decision = null)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetWithDetailsForUpdateAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Preserve CaseTypeId from decision if provided (for CompleteRequestAsync flow)
        if (decision != null && decision.CaseTypeId > 0)
            request.CaseTypeId = decision.CaseTypeId;

        // Set deadline to 30 days from now (BR05)
        request.CompletionDeadline = DateTime.UtcNow.AddDays(30);
        request.RequestStatusId = RequestStatusIds.PendingCompletion;
        request.ModifiedDate = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completion requested for request {RequestId}, deadline: {Deadline}", requestId, request.CompletionDeadline);

        // Send completion request notifications
        _ = SendCompletionRequestNotificationsAsync(request, cancellationToken);

        return MapToViewModel(request);
    }

    /// <summary>
    /// Sends case registration request to judge desk.
    /// </summary>
    public async Task<object> SendToJudgeDeskAsync(int requestId, CancellationToken cancellationToken = default, RequestDecisionDTO? decision = null)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetWithDetailsForUpdateAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        // Preserve CaseTypeId from decision if provided (for CompleteRequestAsync flow)
        if (decision != null && decision.CaseTypeId > 0)
            request.CaseTypeId = decision.CaseTypeId;

        request.RequestStatusId = RequestStatusIds.OnJudgeDesk;
        request.ModifiedDate = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Request {RequestId} sent to judge desk", requestId);

        return MapToViewModel(request);
    }

    /// <summary>
    /// Completes a pending completion request.
    /// </summary>
    public async Task<object> CompleteCompletionAsync(int requestId, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        var request = await _requestRepository.GetWithDetailsForUpdateAsync(requestId, cancellationToken);
        if (request == null)
            throw new InvalidOperationException($"Request {requestId} not found.");

        if (request.RequestStatusId != RequestStatusIds.PendingCompletion)
            throw new InvalidOperationException("Request not in PendingCompletion state.");

        request.RequestStatusId = RequestStatusIds.UnderReview;
        request.ModifiedDate = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completion submitted for request {RequestId}, moving to UnderReview", requestId);

        return MapToViewModel(request);
    }

    /// <summary>
    /// Processes expired completion requests (BR05: Auto-rejection after 30 days).
    /// Runs daily via background service.
    /// </summary>
    public async Task ProcessExpiredCompletionRequestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var expiredRequests = await _requestRepository.GetPendingCompletionExpiredAsync(cancellationToken);

            if (!expiredRequests.Any())
            {
                _logger.LogInformation("No expired completion requests to process");
                return;
            }

            _logger.LogInformation("Processing {Count} expired completion requests", expiredRequests.Count());

            foreach (var request in expiredRequests)
            {
                try
                {
                    request.RequestStatusId = RequestStatusIds.AutoRejected;
                    request.RejectionReason = "انتهت مهلة الاستكمال (30 يوم) - تم الرفض تلقائياً";
                    request.ModifiedDate = DateTime.UtcNow;

                    await _requestRepository.UpdateAsync(request, cancellationToken);

                    _logger.LogInformation("Auto-rejected request {RequestId} due to expired deadline", request.Id);

                    // Send auto-rejection notifications
                    _ = SendAutoRejectionNotificationsAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expired request {RequestId}", request.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessExpiredCompletionRequestsAsync");
        }
    }

    /// <summary>
    /// Completes request processing with final decision (unified endpoint for all actions).
    /// Validates request data, updates CaseTypeId, executes appropriate action method.
    /// Validates: Classifications (ERR005), Defendants (ERR002), Attachments (ERR003), Subject (ERR006), Evidence (ERR007)
    /// </summary>
    public async Task<object> CompleteRequestAsync(int requestId, RequestDecisionDTO decision, CancellationToken cancellationToken = default)
    {
        if (requestId <= 0)
            throw new ArgumentException("Invalid request ID.", nameof(requestId));

        if (decision == null)
            throw new ArgumentNullException(nameof(decision));

        // Load request with details
        var request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken);
        if (request == null || request.IsDeleted)
            throw new InvalidOperationException($"الطلب {requestId} غير موجود");

        // Validate request is in valid state for completion (Draft, New, or PendingCompletion)
        if (request.RequestStatusId != RequestStatusIds.Draft &&
            request.RequestStatusId != RequestStatusIds.New &&
            request.RequestStatusId != RequestStatusIds.PendingCompletion)
            throw new InvalidOperationException(
                "لا يمكن إنهاء الطلب. الحالة الحالية لا تسمح بهذا الإجراء");

        // Store previous status for workflow history
        int previousStatus = request.RequestStatusId;

        // ERR_CASE_TYPE: Case Type ALWAYS required (for ALL decision types)
        if (decision.CaseTypeId <= 0 || decision.CaseTypeId > 2)
            throw new InvalidOperationException("نوع الدعوى مطلوب");

        // **CRITICAL**: Update request with CaseTypeId before executing action
        request.CaseTypeId = decision.CaseTypeId;
        request.ModifiedDate = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // **CRITICAL**: Clear change tracker and reload to get fresh data for validation
        _unitOfWork.ClearChangeTracker();
        request = await _requestRepository.GetWithDetailsAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"الطلب {requestId} غير موجود");

        // Validate for Register and SendToJudge decisions
        if (decision.DecisionType == "Register" || decision.DecisionType == "SendToJudge")
        {
            // Check at least one plaintiff exists
            if (!request.CaseRequestPlaintiffs?.Any(p => !p.IsDeleted) ?? true)
                throw new InvalidOperationException("يجب إدخال مدعى واحد على الأقل");

            // ERR005: Check at least one classification
            if (!request.Classifications?.Any(c => !c.IsDeleted) ?? true)
                throw new InvalidOperationException("يجب تحديد تصنيف واحد على الأقل للدعوى");

            // ERR002: Check at least one defendant
            if (!request.CaseRequestDefendants?.Any(d => !d.IsDeleted) ?? true)
                throw new InvalidOperationException("يجب تحديد مدعى عليه واحد على الأقل");

            // ERR006: Check at least one request attachment
            if (!request.Attachments?.Any(a => !a.IsDeleted) ?? true)
                throw new InvalidOperationException("يجب إضافة مرفق واحد على الأقل");

            // ERR010: Check that all plaintiffs have required attachments
            var plaintiffsWithoutAttachments = request.CaseRequestPlaintiffs
                .Where(crp => !crp.IsDeleted && crp.Plaintiff != null)
                .Where(crp => !crp.Plaintiff!.Attachments?.Any(a => !a.IsDeleted) ?? true)
                .ToList();

            if (plaintiffsWithoutAttachments.Any())
            {
                throw new InvalidOperationException("يجب إدخال المرفقات بشكل صحيح");
            }

            // Corporate Plaintiff Rule: Check corporate plaintiffs have representatives
            // Corporate types: 3, 4, 5, 6, 7, 8 (Business, Company, Gov Agency, NGO, Waqf)
            var corporatePlaintiffsWithoutReps = request.CaseRequestPlaintiffs
                .Where(crp => !crp.IsDeleted && crp.Plaintiff != null)
                .Where(crp => PlaintiffTypeIds.IsCorporate(crp.Plaintiff!.PlaintiffTypeId))
                .Where(crp => !crp.Plaintiff!.Representatives?.Any(r => !r.IsDeleted) ?? true)
                .Select(crp => new
                {
                    PlaintiffId = crp.Plaintiff!.Id,
                    TypeName = crp.Plaintiff.PlaintiffType?.NameAr ?? "غير محدد"
                })
                .ToList();

            if (corporatePlaintiffsWithoutReps.Any())
            {
                throw new InvalidOperationException("يجب إدخال ممثّل للجهات الاعتبارية");
            }

            // Check same person is not both plaintiff and defendant
            // Compare by IdentityNumber and IdentityTypeId
            var plaintiffIdentities = request.CaseRequestPlaintiffs
                .Where(crp => !crp.IsDeleted && crp.Plaintiff != null)
                .Where(crp => !string.IsNullOrWhiteSpace(crp.Plaintiff!.IdentityNumber))
                .Select(crp => new
                {
                    IdentityNumber = crp.Plaintiff!.IdentityNumber,
                    IdentityTypeId = crp.Plaintiff.IdentityTypeId
                })
                .Distinct()
                .ToList();

            var defendantIdentities = request.CaseRequestDefendants
                .Where(crd => !crd.IsDeleted && crd.Defendant != null)
                .Where(crd => !string.IsNullOrWhiteSpace(crd.Defendant!.IdentityNumber))
                .Select(crd => new
                {
                    IdentityNumber = crd.Defendant!.IdentityNumber,
                    IdentityTypeId = crd.Defendant.IdentityTypeId
                })
                .Distinct()
                .ToList();

            var duplicateIdentities = plaintiffIdentities
                .Where(p => defendantIdentities.Any(d =>
                    d.IdentityNumber == p.IdentityNumber &&
                    d.IdentityTypeId == p.IdentityTypeId))
                .ToList();

            if (duplicateIdentities.Any())
            {
                throw new InvalidOperationException("يوجد مدّعي و مدّعى عليه بنفس البيانات");
            }

            // Check at least one plaintiff is marked as applicant (request submitter)
            // Per BC07: Only Individual plaintiffs (types 1-2) can be applicants
            var hasApplicant = request.CaseRequestPlaintiffs
                .Where(crp => !crp.IsDeleted && crp.Plaintiff != null)
                .Any(crp => crp.Plaintiff!.IsApplicant == true);

            if (!hasApplicant)
            {
                throw new InvalidOperationException("يجب اختيار مقدم الطلب");
            }
        }

        // Route to appropriate action method based on decision type
        object result = null!;
        switch (decision.DecisionType)
        {
            case "Register":
                result = await RegisterCaseAsync(requestId, cancellationToken, decision);
                break;

            case "SendToJudge":
                result = await SendToJudgeDeskAsync(requestId, cancellationToken, decision);
                break;

            case "Reject":
                result = await RejectRequestAsync(requestId, decision.Notes ?? "تم الرفض", cancellationToken, decision);
                break;

            case "RequestCompletion":
                result = await RequestCompletionAsync(requestId, decision.Deficiencies, cancellationToken, decision);
                break;

            default:
                throw new InvalidOperationException($"نوع قرار غير صالح: {decision.DecisionType}");
        }

        // **CRITICAL FIX**: Reload request to get updated status from action method
        // Action methods load their own fresh copies and update status in DB,
        // but the original request object in this method is stale.
        // We must clear the tracker and reload to get the correct new status.
        _unitOfWork.ClearChangeTracker();
        request = await _requestRepository.GetByIdAsync(requestId, cancellationToken)
            ?? throw new InvalidOperationException($"الطلب {requestId} غير موجود بعد تنفيذ الإجراء");

        // Parse result to get new status ID
        var resultDict = result as dynamic;
        if (result != null)
        {
            // Save workflow history for audit trail
            try
            {
                await SaveWorkflowHistoryAsync(
                    requestId,
                    previousStatus,
                    request.RequestStatusId, // ✅ NOW CORRECT: Fresh request with updated status
                    decision.Notes,
                    cancellationToken);

                _logger.LogInformation(
                    "Workflow history saved for request {RequestId}: {PreviousStatus} -> {NewStatus}, Notes: {HasNotes}",
                    requestId,
                    previousStatus,
                    request.RequestStatusId,
                    !string.IsNullOrEmpty(decision.Notes) ? "Yes" : "No");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving workflow history for request {RequestId}", requestId);
                // Continue - don't fail if workflow history fails
            }
        }

        return result;
    }

    /// <summary>
    /// Save workflow history record for audit trail
    /// </summary>
    private async Task SaveWorkflowHistoryAsync(
        int requestId,
        int previousStatusId,
        int newStatusId,
        string? notes,
        CancellationToken cancellationToken)
    {
        try
        {
            var workflowRepository = _unitOfWork.GetRepository<CaseRequestWorkflow>();

            var workflow = new CaseRequestWorkflow
            {
                CaseRegistrationRequestId = requestId,
                PreviousStatusId = previousStatusId,
                NewStatusId = newStatusId,
                Notes = notes,
                ActionDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            await workflowRepository.AddAsync(workflow, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Workflow history saved for request {RequestId}: {PreviousStatus} -> {NewStatus}",
                requestId, previousStatusId, newStatusId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SaveWorkflowHistoryAsync for request {RequestId}", requestId);
            throw;
        }
    }

    /// <summary>
    /// Validates if a state transition and action are valid.
    /// </summary>
    public async Task<bool> ValidateStateTransitionAsync(int currentStatusId, string action)
    {
        // Final statuses (OnJudgeDesk-7, Registered-4, Rejected-5) are intentionally NOT included
        // Requests in final statuses cannot transition to other statuses
        var validTransitions = new Dictionary<int, List<string>>
        {
            { RequestStatusIds.Draft, new List<string> { "Submit", "Register", "SendToJudge", "Reject" } },
            { RequestStatusIds.New, new List<string> { "Register", "SendToJudge", "Reject", "RequestCompletion" } },
            { RequestStatusIds.PendingCompletion, new List<string> { "Register", "SendToJudge", "Reject" } },
            { RequestStatusIds.UnderReview, new List<string> { "Accept", "Reject" } }
        };

        if (!validTransitions.ContainsKey(currentStatusId))
            throw new InvalidOperationException($"No actions available for status {currentStatusId}");

        if (!validTransitions[currentStatusId].Contains(action))
            throw new InvalidOperationException($"Action '{action}' not valid for status {currentStatusId}");

        return await Task.FromResult(true);
    }

    #region Helper Methods

    /// <summary>
    /// Sends registration notifications via SMS and Email.
    /// </summary>
    private async Task SendRegistrationNotificationsAsync(CaseRegistrationRequest request, CaseRegistrationResult result, CancellationToken cancellationToken)
    {
        try
        {
            var applicant = request.CaseRequestPlaintiffs?.FirstOrDefault(p => p.Plaintiff?.IsApplicant ?? false)?.Plaintiff;
            if (applicant == null)
                return;

            var templateParams = new SmsTemplateParameters
            {
                RequestNumber = request.Id.ToString(),
                CaseNumber = result.CaseNumber,
                CourtName = request.Court?.NameAr
            };

            // Send SMS
            if (!string.IsNullOrEmpty(applicant.MobileNumber))
            {
                await _smsService.SendTemplatedSmsAsync(
                    applicant.MobileNumber,
                    SmsTemplate.REQUEST_REGISTERED,
                    templateParams,
                    cancellationToken);
            }

            // Send Email
            if (!string.IsNullOrEmpty(applicant.Email))
            {
                var emailParams = new EmailTemplateParameters
                {
                    RecipientName = GetPlaintiffFullName(applicant),
                    RequestNumber = request.Id.ToString(),
                    CaseNumber = result.CaseNumber,
                    RegistrationNumber = result.RegistrationNumber,
                    CourtName = request.Court?.NameAr
                };

                await _emailService.SendTemplatedEmailAsync(
                    applicant.Email,
                    "case_registered",
                    emailParams,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending registration notifications for request {RequestId}", request.Id);
        }
    }

    /// <summary>
    /// Sends rejection notifications.
    /// </summary>
    private async Task SendRejectionNotificationsAsync(CaseRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var applicant = request.CaseRequestPlaintiffs?.FirstOrDefault(p => p.Plaintiff?.IsApplicant ?? false)?.Plaintiff;
            if (applicant == null)
                return;

            var templateParams = new SmsTemplateParameters
            {
                RequestNumber = request.Id.ToString(),
                RejectionReason = request.RejectionReason
            };

            if (!string.IsNullOrEmpty(applicant.MobileNumber))
            {
                await _smsService.SendTemplatedSmsAsync(
                    applicant.MobileNumber,
                    SmsTemplate.REQUEST_REJECTED,
                    templateParams,
                    cancellationToken);
            }

            if (!string.IsNullOrEmpty(applicant.Email))
            {
                var emailParams = new EmailTemplateParameters
                {
                    RecipientName = GetPlaintiffFullName(applicant),
                    RequestNumber = request.Id.ToString(),
                    RejectionReason = request.RejectionReason
                };

                await _emailService.SendTemplatedEmailAsync(
                    applicant.Email,
                    "request_rejected",
                    emailParams,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending rejection notifications for request {RequestId}", request.Id);
        }
    }

    /// <summary>
    /// Sends completion request notifications.
    /// </summary>
    private async Task SendCompletionRequestNotificationsAsync(CaseRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var applicant = request.CaseRequestPlaintiffs?.FirstOrDefault(p => p.Plaintiff?.IsApplicant ?? false)?.Plaintiff;
            if (applicant == null)
                return;

            var templateParams = new SmsTemplateParameters
            {
                RequestNumber = request.Id.ToString(),
                Deadline = request.CompletionDeadline
            };

            if (!string.IsNullOrEmpty(applicant.MobileNumber))
            {
                await _smsService.SendTemplatedSmsAsync(
                    applicant.MobileNumber,
                    SmsTemplate.COMPLETION_REQUIRED,
                    templateParams,
                    cancellationToken);
            }

            if (!string.IsNullOrEmpty(applicant.Email))
            {
                var emailParams = new EmailTemplateParameters
                {
                    RecipientName = GetPlaintiffFullName(applicant),
                    RequestNumber = request.Id.ToString(),
                    Deadline = request.CompletionDeadline
                };

                await _emailService.SendTemplatedEmailAsync(
                    applicant.Email,
                    "completion_required",
                    emailParams,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending completion request notifications for request {RequestId}", request.Id);
        }
    }

    /// <summary>
    /// Sends auto-rejection notifications.
    /// </summary>
    private async Task SendAutoRejectionNotificationsAsync(CaseRegistrationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var applicant = request.CaseRequestPlaintiffs?.FirstOrDefault(p => p.Plaintiff?.IsApplicant ?? false)?.Plaintiff;
            if (applicant == null)
                return;

            var templateParams = new SmsTemplateParameters
            {
                RequestNumber = request.Id.ToString()
            };

            if (!string.IsNullOrEmpty(applicant.MobileNumber))
            {
                await _smsService.SendTemplatedSmsAsync(
                    applicant.MobileNumber,
                    SmsTemplate.AUTO_REJECTED,
                    templateParams,
                    cancellationToken);
            }

            if (!string.IsNullOrEmpty(applicant.Email))
            {
                var emailParams = new EmailTemplateParameters
                {
                    RecipientName = GetPlaintiffFullName(applicant),
                    RequestNumber = request.Id.ToString()
                };

                await _emailService.SendTemplatedEmailAsync(
                    applicant.Email,
                    "request_rejected",
                    emailParams,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auto-rejection notifications for request {RequestId}", request.Id);
        }
    }

    /// <summary>
    /// Maps CaseRegistrationRequest to view model.
    /// </summary>
    private static object MapToViewModel(CaseRegistrationRequest request)
    {
        return new
        {
            id = request.Id,
            requestStatusId = request.RequestStatusId,
            requestStatusName = GetStatusName(request.RequestStatusId),
            courtId = request.CourtId,
            courtName = request.Court?.NameAr ?? "",
            subject = request.Subject,
            evidence = request.Evidence,
            submissionDate = request.SubmissionDate,
            completionDeadline = request.CompletionDeadline,
            rejectionReason = request.RejectionReason,
            caseNumber = request.CaseNumber,
            registrationNumber = request.RegistrationNumber,
            registrationDate = request.RegistrationDate,
            createdDate = request.CreatedDate,
            modifiedDate = request.ModifiedDate,
            caseTypeId = request.CaseTypeId,
            caseTypeName = request.CaseType?.NameAr
        };
    }

    /// <summary>
    /// Gets full name from Plaintiff entity (concatenates name components).
    /// </summary>
    private static string GetPlaintiffFullName(Plaintiff plaintiff)
    {
        if (plaintiff == null)
            return "";

        var nameParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(plaintiff.FirstName))
            nameParts.Add(plaintiff.FirstName);
        if (!string.IsNullOrWhiteSpace(plaintiff.FatherName))
            nameParts.Add(plaintiff.FatherName);
        if (!string.IsNullOrWhiteSpace(plaintiff.GrandfatherName))
            nameParts.Add(plaintiff.GrandfatherName);
        if (!string.IsNullOrWhiteSpace(plaintiff.FamilyName))
            nameParts.Add(plaintiff.FamilyName);

        return string.Join(" ", nameParts);
    }

    /// <summary>
    /// Gets address from Plaintiff entity.
    /// </summary>
    private static string? GetPlaintiffAddress(Plaintiff? plaintiff)
    {
        if (plaintiff == null)
            return null;

        // Return formatted address if available
        return plaintiff.SelectedAddress?.FullAddress ?? plaintiff.ResidenceAddress?.FullAddress ?? null;
    }

    /// <summary>
    /// Gets status name from status ID.
    /// </summary>
    private static string GetStatusName(int statusId) => statusId switch
    {
        1 => "Draft",
        2 => "New",
        3 => "UnderReview",
        4 => "Registered",
        5 => "Rejected",
        6 => "PendingCompletion",
        7 => "OnJudgeDesk",
        8 => "Completed",
        9 => "AutoRejected",
        10 => "Cancelled",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets court Arabic name by court ID.
    /// In production, this should fetch from database.
    /// </summary>
    private static string GetCourtNameById(int courtId) => courtId switch
    {
        1 => "المحكمة الإدارية بالرياض",
        2 => "المحكمة الإدارية بجدة",
        3 => "المحكمة الإدارية بمكة المكرمة",
        4 => "المحكمة الإدارية بالدمام",
        5 => "المحكمة الإدارية بالمدينة المنورة",
        _ => $"المحكمة الإدارية {courtId}"
    };

    #endregion
}
