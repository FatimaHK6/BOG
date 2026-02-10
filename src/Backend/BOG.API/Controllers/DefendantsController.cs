using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration.Defendant;
using BOG.VM.Defendant;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Defendant management API controller.
/// Handles CRUD operations for defendants in case registration requests.
/// Follows Single Responsibility Principle and Dependency Inversion Principle.
/// </summary>
[ApiController]
[Route("api/case-requests")]
public class DefendantsController : ControllerBase
{
    private readonly IDefendantBL _defendantBL;
    private readonly ILogger<DefendantsController> _logger;

    public DefendantsController(IDefendantBL defendantBL, ILogger<DefendantsController> logger)
    {
        _defendantBL = defendantBL ?? throw new ArgumentNullException(nameof(defendantBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all defendants for a case registration request.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of defendants for the request</returns>
    [HttpGet("{requestId}/defendants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DefendantListVM>>> GetDefendants(
        [FromRoute] int requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var defendants = await _defendantBL.GetDefendantsByRequestIdAsync(requestId, cancellationToken);
            return Ok(defendants);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Request not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defendants for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving defendants." });
        }
    }

    /// <summary>
    /// Creates a new defendant for a case registration request.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="createDto">The defendant data to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created defendant</returns>
    [HttpPost("{requestId}/defendants")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DefendantVM>> CreateDefendant(
        [FromRoute] int requestId,
        [FromBody] DefendantCreateDTO createDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var defendant = await _defendantBL.CreateDefendantAsync(requestId, createDto, cancellationToken);
            _logger.LogInformation("Defendant created with ID {DefendantId} for request {RequestId}",
                defendant.Id, requestId);

            return CreatedAtAction(nameof(GetDefendantById), new { id = defendant.Id }, defendant);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ERR013"))
        {
            _logger.LogWarning("Duplicate defendant: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, errorCode = "ERR013" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create defendant: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating defendant for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the defendant." });
        }
    }

    /// <summary>
    /// Gets a defendant by ID.
    /// </summary>
    /// <param name="id">The defendant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The defendant details</returns>
    [HttpGet("defendants/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DefendantVM>> GetDefendantById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid defendant ID." });

            var defendant = await _defendantBL.GetDefendantByIdAsync(id, cancellationToken);
            if (defendant == null)
                return NotFound(new { message = $"Defendant with ID {id} not found." });

            return Ok(defendant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defendant with ID {DefendantId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the defendant." });
        }
    }

    /// <summary>
    /// Updates a defendant.
    /// </summary>
    /// <param name="id">The defendant ID to update</param>
    /// <param name="updateDto">The updated defendant data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated defendant</returns>
    [HttpPut("defendants/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DefendantVM>> UpdateDefendant(
        [FromRoute] int id,
        [FromBody] DefendantUpdateDTO updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid defendant ID." });

            var defendant = await _defendantBL.UpdateDefendantAsync(id, updateDto, cancellationToken);
            _logger.LogInformation("Defendant {DefendantId} updated", id);
            return Ok(defendant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update defendant: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating defendant {DefendantId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the defendant." });
        }
    }

    /// <summary>
    /// Deletes a defendant (soft delete).
    /// </summary>
    /// <param name="id">The defendant ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("defendants/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDefendant(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid defendant ID." });

            await _defendantBL.DeleteDefendantAsync(id, cancellationToken);
            _logger.LogInformation("Defendant {DefendantId} deleted", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete defendant: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defendant {DefendantId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the defendant." });
        }
    }
}
