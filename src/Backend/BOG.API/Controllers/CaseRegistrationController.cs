using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using BOG.VM.Shared;
using CaseRegistrationRequestListVM = BOG.VM.CaseRegistrationRequest.CaseRegistrationRequestListVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BOG.API.Controllers;

/// <summary>
/// Case registration request management API controller.
/// Handles the lifecycle of case registration requests including creation, submission, and actions.
/// Follows Single Responsibility Principle and Dependency Inversion Principle.
/// </summary>
[ApiController]
[Route("api/case-requests")]
public class CaseRegistrationController : ControllerBase
{
    private readonly ICaseRegistrationBL _caseRegistrationBL;
    private readonly IRequestActionBL _requestActionBL;
    private readonly ILogger<CaseRegistrationController> _logger;

    public CaseRegistrationController(
        ICaseRegistrationBL caseRegistrationBL,
        IRequestActionBL requestActionBL,
        ILogger<CaseRegistrationController> logger)
    {
        _caseRegistrationBL = caseRegistrationBL ?? throw new ArgumentNullException(nameof(caseRegistrationBL));
        _requestActionBL = requestActionBL ?? throw new ArgumentNullException(nameof(requestActionBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new case registration request (Draft status).
    /// </summary>
    /// <param name="createDto">The case registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created case registration request</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CaseRegistrationRequestVM>> CreateRequest(
        [FromBody] CaseRegistrationCreateDTO createDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = await _caseRegistrationBL.CreateRequestAsync(createDto, cancellationToken);
            _logger.LogInformation("Case registration request created with ID {RequestId}", request.Id);
            return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating case registration request");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the request." });
        }
    }

    /// <summary>
    /// Gets a case registration request by ID.
    /// </summary>
    /// <param name="id">The request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The request details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseRegistrationRequestVM>> GetRequest(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var request = await _caseRegistrationBL.GetRequestByIdAsync(id, cancellationToken);
            if (request == null)
                return NotFound(new { message = $"Request with ID {id} not found." });

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving request with ID {RequestId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the request." });
        }
    }

    /// <summary>
    /// Gets a case registration request with all details (plaintiffs, defendants, classifications, attachments, etc.).
    /// </summary>
    /// <param name="id">The request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The complete request details including all related entities</returns>
    [HttpGet("{id}/details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseRegistrationRequestDetailsVM>> GetRequestDetails(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var request = await _caseRegistrationBL.GetRequestWithDetailsAsync(id, cancellationToken);
            if (request == null)
                return NotFound(new { message = $"Request with ID {id} not found." });

            return Ok(request);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving request details with ID {RequestId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the request details." });
        }
    }

    /// <summary>
    /// Updates a case registration request (Draft status only).
    /// </summary>
    /// <param name="id">The request ID to update</param>
    /// <param name="updateDto">The updated request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated request</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseRegistrationRequestVM>> UpdateRequest(
        [FromRoute] int id,
        [FromBody] CaseRegistrationUpdateDTO updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var request = await _caseRegistrationBL.UpdateRequestAsync(id, updateDto, cancellationToken);
            _logger.LogInformation("Request {RequestId} updated", id);
            return Ok(request);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning("Validation failed for request {RequestId}: {Message}", id, argEx.Message);
            return BadRequest(new { message = argEx.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update request: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error updating request {RequestId}", id);
            var innerMessage = dbEx.InnerException?.Message ?? "";

            if (innerMessage.Contains("FK_"))
                return BadRequest(new { message = "خطأ في البيانات المرجعية: تأكد من صحة المحكمة ونوع الدعوى" });
            if (innerMessage.Contains("String or binary data would be truncated"))
                return BadRequest(new { message = "البيانات المدخلة تتجاوز الحد المسموح" });

            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "خطأ في قاعدة البيانات" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating request {RequestId}: {ExceptionType}",
                id, ex.GetType().Name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the request." });
        }
    }

    /// <summary>
    /// Submits a case registration request for review (Draft -> New).
    /// Validates all business rules before submission.
    /// </summary>
    /// <param name="id">The request ID to submit</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The submitted request with New status</returns>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseRegistrationRequestVM>> SubmitRequest(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var request = await _caseRegistrationBL.SubmitRequestAsync(id, cancellationToken);
            _logger.LogInformation("Request {RequestId} submitted", id);
            return Ok(request);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ERR"))
        {
            _logger.LogWarning("Validation failed for request {RequestId}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to submit request: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting request {RequestId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while submitting the request." });
        }
    }

    /// <summary>
    /// Takes an action on a case registration request (Register, Reject, SendToJudge, RequestCompletion).
    /// </summary>
    /// <param name="id">The request ID</param>
    /// <param name="actionDto">The action to take</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The request with updated status</returns>
    [HttpPost("{id}/action")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseRegistrationRequestVM>> TakeAction(
        [FromRoute] int id,
        [FromBody] TakeActionDTO actionDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var request = await _requestActionBL.TakeActionAsync(id, actionDto, cancellationToken);
            _logger.LogInformation("Action {Action} taken on request {RequestId}", actionDto.Action, id);
            return Ok(request);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Action validation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error taking action on request {RequestId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while processing the action." });
        }
    }

    /// <summary>
    /// Complete request processing with final decision.
    /// POST /api/case-requests/{id}/complete
    /// Unified endpoint for Register, SendToJudge, Reject, RequestCompletion decisions
    /// ALWAYS requires Case Type (نوع الدعوى) for all decision types
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseRegistrationRequestVM>> CompleteRequest(
        [FromRoute] int id,
        [FromBody] RequestDecisionDTO decision,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var result = await _requestActionBL.CompleteRequestAsync(id, decision, cancellationToken);
            _logger.LogInformation("Request {RequestId} completed with decision: {Decision}",
                id, decision.DecisionType);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot complete request {RequestId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing request {RequestId}", id);
            return StatusCode(500, new { message = "حدث خطأ أثناء معالجة الطلب" });
        }
    }

    /// <summary>
    /// Gets case registration requests with pagination and optional filtering.
    /// Accepts query parameters for pagination and filtering.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="requestNumber">Filter by request number (optional)</param>
    /// <param name="status">Filter by status (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of requests</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CaseRegistrationRequestListVM>>> GetRequests(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? requestNumber = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber <= 0)
                return BadRequest(new { message = "Page number must be greater than 0." });

            if (pageSize <= 0 || pageSize > 100)
                return BadRequest(new { message = "Page size must be between 1 and 100." });

            // Create search criteria from query parameters
            var searchDto = new SearchRequestDTO
            {
                CaseNumber = requestNumber,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Parse status filter if provided
            if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out int statusId))
                searchDto.StatusId = statusId;

            var result = await _caseRegistrationBL.SearchRequestsAsync(searchDto, pageNumber, pageSize, cancellationToken);
            _logger.LogInformation("Retrieved requests: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid parameters: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving requests");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving requests." });
        }
    }

    /// <summary>
    /// Searches case registration requests with filtering and pagination.
    /// </summary>
    /// <param name="searchDto">Search filters and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of requests matching search criteria</returns>
    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CaseRegistrationRequestListVM>>> SearchRequests(
        [FromBody] SearchRequestDTO searchDto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate pagination parameters
            if (searchDto.PageNumber <= 0)
                return BadRequest(new { message = "Page number must be greater than 0." });

            if (searchDto.PageSize <= 0 || searchDto.PageSize > 100)
                return BadRequest(new { message = "Page size must be between 1 and 100." });

            var result = await _caseRegistrationBL.SearchRequestsAsync(searchDto, searchDto.PageNumber, searchDto.PageSize, cancellationToken);
            _logger.LogInformation("Searched requests with filters: PageNumber={PageNumber}, PageSize={PageSize}",
                searchDto.PageNumber, searchDto.PageSize);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid search parameters: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching requests");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while searching requests." });
        }
    }
}
