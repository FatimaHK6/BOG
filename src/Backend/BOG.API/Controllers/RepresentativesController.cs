using BOG.BL.Interfaces;
using BOG.DTO.Representative;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Representative management API controller.
/// Handles representative CRUD operations for plaintiffs.
/// </summary>
[ApiController]
[Route("api")]
public class RepresentativesController : ControllerBase
{
    private readonly IRepresentativeBL _representativeBL;
    private readonly ILogger<RepresentativesController> _logger;

    public RepresentativesController(
        IRepresentativeBL representativeBL,
        ILogger<RepresentativesController> logger)
    {
        _representativeBL = representativeBL ?? throw new ArgumentNullException(nameof(representativeBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all representatives for a plaintiff.
    /// </summary>
    [HttpGet("plaintiffs/{plaintiffId}/representatives")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRepresentativesByPlaintiffId(
        [FromRoute] int plaintiffId,
        CancellationToken cancellationToken)
    {
        try
        {
            var representatives = await _representativeBL.GetByPlaintiffIdAsync(plaintiffId, cancellationToken);
            return Ok(representatives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving representatives for plaintiff {PlaintiffId}", plaintiffId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع بيانات الممثلين" });
        }
    }

    /// <summary>
    /// Creates a new representative for a plaintiff.
    /// Validates representative type allowed for plaintiff type.
    /// </summary>
    [HttpPost("plaintiffs/{plaintiffId}/representatives")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateRepresentative(
        [FromRoute] int plaintiffId,
        [FromBody] RepresentativeCreateDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var representative = await _representativeBL.CreateAsync(plaintiffId, dto, cancellationToken);
            _logger.LogInformation("Representative created with ID: {RepId} for plaintiff {PlaintiffId}",
                representative.Id, plaintiffId);
            return CreatedAtAction(nameof(GetRepresentativeById), new { id = representative.Id }, representative);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create representative: {Message}", ex.Message);

            // Return appropriate status code based on error type
            if (ex.Message.Contains("ERR008") || ex.Message.Contains("ERR012"))
                return Conflict(new { message = ex.Message });

            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating representative for plaintiff {PlaintiffId}", plaintiffId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء إنشاء الممثل" });
        }
    }

    /// <summary>
    /// Gets a representative by ID with full details.
    /// </summary>
    [HttpGet("representatives/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetRepresentativeById([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var representative = await _representativeBL.GetByIdAsync(id, cancellationToken);
            if (representative == null)
                return NotFound(new { message = $"الممثل رقم {id} غير موجود" });

            return Ok(representative);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving representative with ID: {RepId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع بيانات الممثل" });
        }
    }

    /// <summary>
    /// Updates a representative.
    /// </summary>
    [HttpPut("representatives/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateRepresentative(
        [FromRoute] int id,
        [FromBody] RepresentativeUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var representative = await _representativeBL.UpdateAsync(id, dto, cancellationToken);
            _logger.LogInformation("Representative with ID {RepId} updated", id);
            return Ok(representative);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update representative: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating representative with ID: {RepId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تحديث بيانات الممثل" });
        }
    }

    /// <summary>
    /// Deletes a representative (soft delete).
    /// </summary>
    [HttpDelete("representatives/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRepresentative([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            await _representativeBL.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Representative with ID {RepId} deleted", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete representative: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting representative with ID: {RepId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء حذف الممثل" });
        }
    }

    /// <summary>
    /// Gets allowed representative types for a plaintiff type.
    /// </summary>
    [HttpGet("plaintiff-types/{plaintiffTypeId}/allowed-representative-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetAllowedRepresentativeTypes([FromRoute] int plaintiffTypeId)
    {
        var allowedTypes = _representativeBL.GetAllowedRepresentativeTypes(plaintiffTypeId);
        return Ok(new { plaintiffTypeId, allowedRepresentativeTypes = allowedTypes });
    }
}
