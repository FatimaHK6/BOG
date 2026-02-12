using BOG.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Case Registration Request API controller.
/// Handles case registration request CRUD operations.
/// </summary>
[ApiController]
[Route("api/case-registration-requests")]
public class CaseRegistrationRequestsController : ControllerBase
{
    private readonly ICaseRegistrationRequestBL _requestBL;
    private readonly ILogger<CaseRegistrationRequestsController> _logger;

    public CaseRegistrationRequestsController(
        ICaseRegistrationRequestBL requestBL,
        ILogger<CaseRegistrationRequestsController> logger)
    {
        _requestBL = requestBL ?? throw new ArgumentNullException(nameof(requestBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all case registration requests.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all case registration requests");
            var requests = await _requestBL.GetAllAsync(cancellationToken);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all requests");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع الطلبات" });
        }
    }

    /// <summary>
    /// Gets all case registration requests for a user.
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetByUserId([FromRoute] int userId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting case registration requests for user {UserId}", userId);
            var requests = await _requestBL.GetByUserIdAsync(userId, cancellationToken);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving requests for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع الطلبات" });
        }
    }

    /// <summary>
    /// Gets a case registration request by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var request = await _requestBL.GetByIdAsync(id, cancellationToken);
            if (request == null)
                return NotFound(new { message = $"الطلب رقم {id} غير موجود" });

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving request {RequestId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع الطلب" });
        }
    }

    /// <summary>
    /// Creates a new draft case registration request.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateDraft(CancellationToken cancellationToken)
    {
        // TODO: Get userId from authentication when implemented
        const int userId = 1; // Default user for development
        try
        {
            _logger.LogInformation("Creating draft case registration request for user {UserId}", userId);
            var request = await _requestBL.CreateDraftAsync(userId, cancellationToken);
            _logger.LogInformation("Created draft request with ID {RequestId}", request.Id);
            // Return simplified response with request ID and status
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, new
            {
                id = request.Id,
                requestStatusId = request.RequestStatusId,
                statusNameAr = request.StatusNameAr,
                createdDate = request.CreatedDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating draft request for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء إنشاء الطلب" });
        }
    }

    /// <summary>
    /// Updates a case registration request (save as draft).
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateRequestDTO dto, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating case registration request {RequestId}", id);
            var result = await _requestBL.UpdateAsync(id, dto?.SaveAsDraft ?? true, cancellationToken);
            if (result == null)
                return NotFound(new { message = $"الطلب رقم {id} غير موجود" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating request {RequestId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تحديث الطلب" });
        }
    }

    /// <summary>
    /// Deletes a case registration request.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            await _requestBL.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Request {RequestId} not found for deletion", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting request {RequestId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء حذف الطلب" });
        }
    }
}

/// <summary>
/// DTO for updating a case registration request.
/// </summary>
public class UpdateRequestDTO
{
    public bool SaveAsDraft { get; set; } = true;
}
