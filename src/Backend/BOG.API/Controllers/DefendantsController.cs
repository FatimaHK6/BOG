using BOG.BL.Interfaces;
using BOG.DTO.Defendant;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Defendant management API controller.
/// </summary>
[ApiController]
[Route("api/case-registration-requests")]
public class DefendantsController : ControllerBase
{
    private readonly IDefendantBL _defendantBL;
    private readonly IValidator<DefendantCreateDTO> _validator;
    private readonly ILogger<DefendantsController> _logger;

    public DefendantsController(
        IDefendantBL defendantBL,
        IValidator<DefendantCreateDTO> validator,
        ILogger<DefendantsController> logger)
    {
        _defendantBL = defendantBL ?? throw new ArgumentNullException(nameof(defendantBL));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all defendants for a case request.
    /// </summary>
    [HttpGet("case-requests/{requestId}/defendants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDefendantsByRequestId([FromRoute] int requestId, CancellationToken cancellationToken)
    {
        try
        {
            var defendants = await _defendantBL.GetByRequestIdAsync(requestId, cancellationToken);
            return Ok(defendants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defendants for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع بيانات المدعى عليهم" });
        }
    }

    /// <summary>
    /// Gets a defendant by ID.
    /// </summary>
    [HttpGet("defendants/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDefendantById([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var defendant = await _defendantBL.GetByIdAsync(id, cancellationToken);
            if (defendant == null)
                return NotFound(new { message = "المدعى عليه غير موجود" });

            return Ok(defendant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defendant {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع بيانات المدعى عليه" });
        }
    }

    /// <summary>
    /// Creates a new defendant for a case request.
    /// </summary>
    [HttpPost("case-requests/{requestId}/defendants")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateDefendant(
        [FromRoute] int requestId,
        [FromBody] DefendantCreateDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate DTO
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { message = "بيانات غير صالحة", errors });
            }

            var defendant = await _defendantBL.CreateAsync(requestId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetDefendantById), new { id = defendant.Id }, defendant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating defendant for request {RequestId}", requestId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating defendant for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء إنشاء المدعى عليه" });
        }
    }

    /// <summary>
    /// Updates a defendant.
    /// </summary>
    [HttpPut("defendants/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateDefendant(
        [FromRoute] int id,
        [FromBody] DefendantCreateDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate DTO
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { message = "بيانات غير صالحة", errors });
            }

            var defendant = await _defendantBL.UpdateAsync(id, dto, cancellationToken);
            if (defendant == null)
                return NotFound(new { message = "المدعى عليه غير موجود" });

            return Ok(defendant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating defendant {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating defendant {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تعديل المدعى عليه" });
        }
    }

    /// <summary>
    /// Deletes a defendant (soft delete).
    /// </summary>
    [HttpDelete("defendants/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDefendant([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _defendantBL.DeleteAsync(id, cancellationToken);
            if (!result)
                return NotFound(new { message = "المدعى عليه غير موجود" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting defendant {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء حذف المدعى عليه" });
        }
    }
}
