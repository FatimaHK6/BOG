using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// API Controller for managing case deficiencies (نواقص الدعوى).
/// Provides endpoints for retrieving and batch updating deficiencies.
/// </summary>
[ApiController]
[Route("api/case-requests/{requestId}/deficiencies")]
public class DeficienciesController : ControllerBase
{
    private readonly IDeficiencyBL _deficiencyBL;
    private readonly ILogger<DeficienciesController> _logger;

    public DeficienciesController(IDeficiencyBL deficiencyBL, ILogger<DeficienciesController> logger)
    {
        _deficiencyBL = deficiencyBL ?? throw new ArgumentNullException(nameof(deficiencyBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all deficiencies for a specific case registration request.
    /// Accessible to all authenticated users.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of deficiencies with type and description details</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeficiencyVM>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DeficiencyVM>>> GetDeficiencies(
        [FromRoute] int requestId,
        CancellationToken ct = default)
    {
        try
        {
            var deficiencies = await _deficiencyBL.GetDeficienciesAsync(requestId, ct);
            return Ok(deficiencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deficiencies for request {RequestId}", requestId);
            return StatusCode(500, new { message = "خطأ في الحصول على النواقص", error = ex.Message });
        }
    }

    /// <summary>
    /// Replaces all deficiencies for a request with the provided list.
    /// Batch update operation: soft deletes all existing deficiencies and creates new ones.
    /// Only accessible to موظف القيد (RegistrationEmployee).
    /// Note: Does NOT change request status or send notifications.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="dto">Batch update DTO containing new deficiencies list</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated list of deficiencies</returns>
    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<DeficiencyVM>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DeficiencyVM>>> UpdateDeficiencies(
        [FromRoute] int requestId,
        [FromBody] DeficienciesBatchUpdateDTO dto,
        CancellationToken ct = default)
    {
        try
        {
            var deficiencies = await _deficiencyBL.UpdateDeficienciesAsync(requestId, dto, ct);
            return Ok(deficiencies);
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
            _logger.LogError(ex, "Error updating deficiencies for request {RequestId}", requestId);
            return StatusCode(500, new { message = "خطأ في تحديث النواقص", error = ex.Message });
        }
    }
}
