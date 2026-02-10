using BOG.DTO.CaseRegistration;

namespace BOG.BL.Interfaces.CaseRegistration;

/// <summary>
/// Business logic service for case registration request state transitions and actions.
/// Handles Register, Reject, SendToJudge, RequestCompletion actions and auto-rejection.
/// Manages state machine transitions and sends notifications.
/// </summary>
public interface IRequestActionBL
{
    /// <summary>
    /// Takes an action on a case registration request (Register/Reject/SendToJudge/RequestCompletion).
    /// Validates state transition and action requirements.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="actionData">Action details (action type, notes, deficiencies)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated case registration request view model</returns>
    Task<object> TakeActionAsync(int requestId, object actionData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a case with the external case management system.
    /// Changes state from New (3) or OnJudgeDesk (5) to Registered (6).
    /// Calls ICaseManagementService.RegisterCaseAsync to create case in external system.
    /// Sends registration notifications (SMS and Email).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registered case registration request view model with CaseNumber and RegistrationNumber</returns>
    Task<object> RegisterCaseAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a case registration request with reason.
    /// Changes state to Rejected (10).
    /// Sends rejection notification (SMS and Email).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="rejectionReason">Reason for rejection</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rejected case registration request view model</returns>
    Task<object> RejectRequestAsync(int requestId, string rejectionReason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests completion of missing documents for a case registration request.
    /// Changes state from New (3) to PendingCompletion (8).
    /// Sets CompletionDeadline to current date + 30 days.
    /// Lists deficiency types to be addressed.
    /// Sends completion request notification (SMS and Email).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="deficiencies">List of deficiency types required</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Case registration request in PendingCompletion state</returns>
    Task<object> RequestCompletionAsync(int requestId, object deficiencies, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends case registration request to judge desk for review.
    /// Changes state from New (3) to OnJudgeDesk (5).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Case registration request on judge desk</returns>
    Task<object> SendToJudgeDeskAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a pending completion request.
    /// Changes state from PendingCompletion (8) to UnderReview (9).
    /// Called when applicant submits required documents before deadline.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Case registration request under review</returns>
    Task<object> CompleteCompletionAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes expired completion requests (BR05: Auto-rejection after 30 days).
    /// Runs daily via background service.
    /// Finds all requests in PendingCompletion state with expired deadline.
    /// Changes state to Rejected (10) with auto-rejection reason.
    /// Sends auto-rejection notifications (SMS and Email).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing async operation</returns>
    Task ProcessExpiredCompletionRequestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes request processing with final decision (unified endpoint for all actions).
    /// Validates request data, updates CaseTypeId, executes appropriate action method.
    /// Used by Registration Clerk (موظف القيد) to finalize request.
    /// Validates: Classifications (ERR005), Defendants (ERR002), Attachments (ERR003), Subject (ERR006), Evidence (ERR007)
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="decision">Decision details (type, case type, notes)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated case registration request view model</returns>
    Task<object> CompleteRequestAsync(int requestId, RequestDecisionDTO decision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a state transition and action are valid.
    /// </summary>
    /// <param name="currentStatusId">Current request status ID</param>
    /// <param name="action">Requested action (Register/Reject/SendToJudge/RequestCompletion/Complete)</param>
    /// <returns>True if transition is valid, throws exception if invalid</returns>
    Task<bool> ValidateStateTransitionAsync(int currentStatusId, string action);
}
