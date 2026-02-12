using BOG.VM.CaseRegistration;
using BOG.VM.Shared;

namespace BOG.BL.Interfaces.CaseRegistration;

/// <summary>
/// Business logic service for case registration request lifecycle management.
/// Handles request creation, updates, submission validation, and state management.
/// </summary>
public interface ICaseRegistrationBL
{
    /// <summary>
    /// Creates a new case registration request in Draft state.
    /// </summary>
    /// <param name="requestData">Case registration request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created case registration request view model</returns>
    Task<CaseRegistrationRequestVM> CreateRequestAsync(object requestData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a case registration request by ID.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Case registration request view model or null if not found</returns>
    Task<CaseRegistrationRequestVM?> GetRequestByIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets case registration request with all related details (plaintiffs, defendants, attachments, etc.).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed case registration request view model</returns>
    Task<CaseRegistrationRequestDetailsVM?> GetRequestWithDetailsAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a case registration request.
    /// Only allowed in Draft and PendingCompletion states.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="requestData">Updated request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated case registration request view model</returns>
    Task<CaseRegistrationRequestVM> UpdateRequestAsync(int requestId, object requestData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a case registration request for review.
    /// Changes state from Draft (1) to New (3).
    /// Validates all business rules (ERR001-007).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Submitted case registration request view model</returns>
    Task<CaseRegistrationRequestVM> SubmitRequestAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates case registration request for submission.
    /// Checks:
    /// - ERR001: At least one plaintiff required
    /// - ERR002: At least one defendant required
    /// - ERR003: Mandatory attachments complete
    /// - ERR004: Applicant specified
    /// - ERR005: Classifications specified
    /// - ERR006: Subject not empty
    /// - ERR007: Evidence not empty
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if valid, throws exception with error details if invalid</returns>
    Task<bool> ValidateForSubmissionAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches case registration requests with optional filters and pagination.
    /// </summary>
    /// <param name="searchCriteria">Search filters (identity number, party name, court ID, status, etc.)</param>
    /// <param name="pageNumber">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of case registration requests</returns>
    Task<PagedResult<CaseRegistrationRequestVM>> SearchRequestsAsync(object searchCriteria, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}
