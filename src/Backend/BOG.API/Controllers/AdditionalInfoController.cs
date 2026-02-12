using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.AdditionalInfo;
using BOG.VM.AdditionalInfo;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Additional Information management API controller.
/// Handles the supplementary information for case registration requests.
/// Supports three types of additional information based on case type.
/// </summary>
[ApiController]
[Route("api/case-requests/{requestId}/additional-info")]
public class AdditionalInfoController : ControllerBase
{
    private readonly IAdditionalInfoBL _additionalInfoBL;
    private readonly ILogger<AdditionalInfoController> _logger;

    public AdditionalInfoController(
        IAdditionalInfoBL additionalInfoBL,
        ILogger<AdditionalInfoController> logger)
    {
        _additionalInfoBL = additionalInfoBL ?? throw new ArgumentNullException(nameof(additionalInfoBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets existing additional information for a case registration request.
    /// Returns all three types of additional info (if any exist).
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Additional information for the request, or empty object if none exists</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdditionalInfoVM?>> GetAdditionalInfo(
        [FromRoute] int requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var additionalInfo = await _additionalInfoBL.GetByRequestIdAsync(requestId, cancellationToken);
            if (additionalInfo == null)
            {
                _logger.LogInformation("No additional info found for request {RequestId}", requestId);
                return Ok(new AdditionalInfoVM { CaseRegistrationRequestId = requestId });
            }

            _logger.LogInformation("Retrieved additional info for request {RequestId}", requestId);
            return Ok(additionalInfo);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving additional info for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving additional information." });
        }
    }

    /// <summary>
    /// Adds or updates additional information for a case registration request.
    /// Handles all three types: Management Decision, Service/Retirement Rights, and Trademark.
    /// All types are optional - the system automatically detects which types have data.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="updateDto">The additional information data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The saved additional information</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdditionalInfoVM>> SaveAdditionalInfo(
        [FromRoute] int requestId,
        [FromBody] AdditionalInfoDTO updateDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            if (updateDto == null)
                return BadRequest(new { message = "Additional info data is required." });

            var result = await _additionalInfoBL.AddOrUpdateAsync(requestId, updateDto, cancellationToken);
            _logger.LogInformation("Additional info saved for request {RequestId}", requestId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to save additional info: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving additional info for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while saving additional information." });
        }
    }

    /// <summary>
    /// Deletes additional information for a case registration request (soft delete).
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAdditionalInfo(
        [FromRoute] int requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            await _additionalInfoBL.DeleteByRequestIdAsync(requestId, cancellationToken);
            _logger.LogInformation("Additional info deleted for request {RequestId}", requestId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete additional info: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting additional info for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting additional information." });
        }
    }
}
