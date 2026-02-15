using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// API Controller for managing related cases (الدعاوى المرتبطة).
/// Provides endpoints for retrieving and batch updating related cases.
/// </summary>
[ApiController]
[Route("api/case-registration-requests/{requestId}/related-cases")]
public class RelatedCasesController : ControllerBase
{
    private readonly IRelatedCaseBL _relatedCaseBL;
    private readonly ILogger<RelatedCasesController> _logger;

    public RelatedCasesController(IRelatedCaseBL relatedCaseBL, ILogger<RelatedCasesController> logger)
    {
        _relatedCaseBL = relatedCaseBL ?? throw new ArgumentNullException(nameof(relatedCaseBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all related cases for a specific case registration request.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of related cases</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RelatedCaseVM>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RelatedCaseVM>>> GetRelatedCases(
        [FromRoute] int requestId,
        CancellationToken ct = default)
    {
        try
        {
            var relatedCases = await _relatedCaseBL.GetRelatedCasesAsync(requestId, ct);
            return Ok(relatedCases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving related cases for request {RequestId}", requestId);
            return StatusCode(500, new { message = "خطأ في الحصول على الدعاوى المرتبطة", error = ex.Message });
        }
    }

    /// <summary>
    /// Replaces all related cases for a request with the provided list.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="dto">Batch update DTO containing new related cases list</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated list of related cases</returns>
    [HttpPut]
    [ProducesResponseType(typeof(IEnumerable<RelatedCaseVM>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RelatedCaseVM>>> UpdateRelatedCases(
        [FromRoute] int requestId,
        [FromBody] RelatedCasesBatchUpdateDTO dto,
        CancellationToken ct = default)
    {
        try
        {
            var relatedCases = await _relatedCaseBL.UpdateRelatedCasesAsync(requestId, dto, ct);
            return Ok(relatedCases);
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
            _logger.LogError(ex, "Error updating related cases for request {RequestId}", requestId);
            return StatusCode(500, new { message = "خطأ في تحديث الدعاوى المرتبطة", error = ex.Message });
        }
    }
}
