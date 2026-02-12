using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// API Controller for managing case claims (طلبات الدعوى).
/// Provides endpoints for retrieving and batch updating claims.
/// </summary>
[ApiController]
[Route("api/case-requests/{requestId}/claims")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimBL _claimBL;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(IClaimBL claimBL, ILogger<ClaimsController> logger)
    {
        _claimBL = claimBL ?? throw new ArgumentNullException(nameof(claimBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all claims for a specific case registration request.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of claims</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClaimVM>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ClaimVM>>> GetClaims(
        [FromRoute] int requestId,
        CancellationToken ct = default)
    {
        try
        {
            var claims = await _claimBL.GetClaimsAsync(requestId, ct);
            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving claims for request {RequestId}", requestId);
            return StatusCode(500, new { message = "خطأ في الحصول على الطلبات", error = ex.Message });
        }
    }

    /// <summary>
    /// Replaces all claims for a request with the provided list.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="dto">Batch update DTO containing new claims list</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated list of claims</returns>
    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<ClaimVM>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ClaimVM>>> UpdateClaims(
        [FromRoute] int requestId,
        [FromBody] ClaimsBatchUpdateDTO dto,
        CancellationToken ct = default)
    {
        try
        {
            var claims = await _claimBL.UpdateClaimsAsync(requestId, dto, ct);
            return Ok(claims);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Request {RequestId} not found", requestId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for request {RequestId}", requestId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating claims for request {RequestId}", requestId);
            return StatusCode(500, new { message = "خطأ في تحديث الطلبات", error = ex.Message });
        }
    }
}
