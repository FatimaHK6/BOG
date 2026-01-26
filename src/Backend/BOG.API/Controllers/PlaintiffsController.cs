using BOG.BL.Interfaces;
using BOG.DTO.Plaintiff;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Plaintiff management API controller.
/// Handles plaintiff CRUD operations, attachments, and address selection.
/// </summary>
[ApiController]
[Route("api")]
public class PlaintiffsController : ControllerBase
{
    private readonly IPlaintiffBL _plaintiffBL;
    private readonly IPlaintiffAttachmentBL _attachmentBL;
    private readonly IValidator<PlaintiffCreateDTO> _plaintiffValidator;
    private readonly ILogger<PlaintiffsController> _logger;

    public PlaintiffsController(
        IPlaintiffBL plaintiffBL,
        IPlaintiffAttachmentBL attachmentBL,
        IValidator<PlaintiffCreateDTO> plaintiffValidator,
        ILogger<PlaintiffsController> logger)
    {
        _plaintiffBL = plaintiffBL ?? throw new ArgumentNullException(nameof(plaintiffBL));
        _attachmentBL = attachmentBL ?? throw new ArgumentNullException(nameof(attachmentBL));
        _plaintiffValidator = plaintiffValidator ?? throw new ArgumentNullException(nameof(plaintiffValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Plaintiff CRUD

    /// <summary>
    /// Gets all plaintiffs for a case request.
    /// </summary>
    [HttpGet("case-requests/{requestId}/plaintiffs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPlaintiffsByRequestId([FromRoute] int requestId, CancellationToken cancellationToken)
    {
        try
        {
            var plaintiffs = await _plaintiffBL.GetByRequestIdAsync(requestId, cancellationToken);
            return Ok(plaintiffs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plaintiffs for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع بيانات المدعين" });
        }
    }

    /// <summary>
    /// Creates a new plaintiff for a case request.
    /// </summary>
    [HttpPost("case-requests/{requestId}/plaintiffs")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreatePlaintiff(
        [FromRoute] int requestId,
        [FromBody] PlaintiffCreateDTO dto,
        CancellationToken cancellationToken)
    {
        // === DEBUG LOGGING START ===
        _logger.LogWarning("=== CREATE PLAINTIFF API CALLED ===");
        _logger.LogWarning("RequestId: {RequestId}", requestId);
        _logger.LogWarning("DTO PlaintiffTypeId: {TypeId}", dto?.PlaintiffTypeId);
        _logger.LogWarning("DTO WaqfName: {WaqfName}", dto?.WaqfName);
        _logger.LogWarning("DTO CourtDeedNumber: {DeedNum}", dto?.CourtDeedNumber);
        _logger.LogWarning("DTO DeedDate: {DeedDate}", dto?.DeedDate);
        _logger.LogWarning("DTO DeedSource: {DeedSource}", dto?.DeedSource);
        _logger.LogWarning("DTO WaqfOversightType: {OversightType}", dto?.WaqfOversightType);
        _logger.LogWarning("DTO WaqfDescription: {Desc}", dto?.WaqfDescription);
        _logger.LogWarning("DTO WaqfAddress: RegionId={RegionId}, CityId={CityId}",
            dto?.WaqfAddress?.RegionId, dto?.WaqfAddress?.CityId);
        // === DEBUG LOGGING END ===

        // Validate the DTO using FluentValidation
        var validationResult = await _plaintiffValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            _logger.LogWarning("=== VALIDATION FAILED ===");
            foreach (var error in validationResult.Errors)
            {
                _logger.LogWarning("Validation Error: {Property} - {Message}", error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(new { errors });
        }

        _logger.LogWarning("=== VALIDATION PASSED, CALLING BL ===");

        try
        {
            var plaintiff = await _plaintiffBL.CreateAsync(requestId, dto, cancellationToken);
            _logger.LogWarning("=== PLAINTIFF CREATED SUCCESSFULLY ===");
            _logger.LogWarning("Plaintiff created with ID: {PlaintiffId} for request {RequestId}",
                plaintiff.Id, requestId);
            return CreatedAtAction(nameof(GetPlaintiffById), new { id = plaintiff.Id }, plaintiff);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create plaintiff: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating plaintiff for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء إنشاء المدعي" });
        }
    }

    /// <summary>
    /// Gets a plaintiff by ID with full details.
    /// </summary>
    [HttpGet("plaintiffs/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPlaintiffById([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var plaintiff = await _plaintiffBL.GetByIdAsync(id, cancellationToken);
            if (plaintiff == null)
                return NotFound(new { message = $"المدعي رقم {id} غير موجود" });

            return Ok(plaintiff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plaintiff with ID: {PlaintiffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع بيانات المدعي" });
        }
    }

    /// <summary>
    /// Updates a plaintiff.
    /// </summary>
    [HttpPut("plaintiffs/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePlaintiff(
        [FromRoute] int id,
        [FromBody] PlaintiffUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var plaintiff = await _plaintiffBL.UpdateAsync(id, dto, cancellationToken);
            _logger.LogInformation("Plaintiff with ID {PlaintiffId} updated", id);
            return Ok(plaintiff);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update plaintiff: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating plaintiff with ID: {PlaintiffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تحديث بيانات المدعي" });
        }
    }

    /// <summary>
    /// Deletes a plaintiff (soft delete).
    /// </summary>
    [HttpDelete("plaintiffs/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePlaintiff([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            await _plaintiffBL.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Plaintiff with ID {PlaintiffId} deleted", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete plaintiff: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plaintiff with ID: {PlaintiffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء حذف المدعي" });
        }
    }

    /// <summary>
    /// Sets a plaintiff as the applicant (مقدم الطلب).
    /// Only Individual types can be applicant (BC07).
    /// </summary>
    [HttpPost("plaintiffs/{id}/set-applicant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetAsApplicant([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var plaintiff = await _plaintiffBL.SetAsApplicantAsync(id, cancellationToken);
            _logger.LogInformation("Plaintiff with ID {PlaintiffId} set as applicant", id);
            return Ok(plaintiff);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to set applicant: {Message}", ex.Message);
            if (ex.Message.Contains("غير موجود"))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting plaintiff {PlaintiffId} as applicant", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تعيين مقدم الطلب" });
        }
    }

    #endregion

    #region Selected Address

    /// <summary>
    /// Gets the selected address for a plaintiff.
    /// </summary>
    [HttpGet("plaintiffs/{id}/selected-address")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetSelectedAddress([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var address = await _plaintiffBL.GetSelectedAddressAsync(id, cancellationToken);
            if (address == null)
                return NotFound(new { message = "لم يتم تحديد عنوان مختار" });

            return Ok(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving selected address for plaintiff {PlaintiffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع العنوان المختار" });
        }
    }

    /// <summary>
    /// Sets the selected address for notifications (العنوان المختار).
    /// Required when plaintiff city differs from court city (BC03).
    /// </summary>
    [HttpPut("plaintiffs/{id}/selected-address")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetSelectedAddress(
        [FromRoute] int id,
        [FromBody] SelectedAddressDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var address = await _plaintiffBL.SetSelectedAddressAsync(id, dto, cancellationToken);
            _logger.LogInformation("Selected address set for plaintiff {PlaintiffId}", id);
            return Ok(address);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to set selected address: {Message}", ex.Message);
            if (ex.Message.Contains("غير موجود"))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting selected address for plaintiff {PlaintiffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تعيين العنوان المختار" });
        }
    }

    #endregion

    #region Attachments

    /// <summary>
    /// Gets all attachments for a plaintiff.
    /// </summary>
    [HttpGet("plaintiffs/{plaintiffId}/attachments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAttachments([FromRoute] int plaintiffId, CancellationToken cancellationToken)
    {
        try
        {
            var attachments = await _attachmentBL.GetByPlaintiffIdAsync(plaintiffId, cancellationToken);
            return Ok(attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for plaintiff {PlaintiffId}", plaintiffId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع المرفقات" });
        }
    }

    /// <summary>
    /// Gets an attachment by ID.
    /// </summary>
    [HttpGet("plaintiffs/{plaintiffId}/attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetAttachmentById(
        [FromRoute] int plaintiffId,
        [FromRoute] int attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var attachment = await _attachmentBL.GetByIdAsync(attachmentId, cancellationToken);
            if (attachment == null || attachment.PlaintiffId != plaintiffId)
                return NotFound(new { message = $"المرفق رقم {attachmentId} غير موجود" });

            return Ok(attachment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachment {AttachmentId}", attachmentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع المرفق" });
        }
    }

    /// <summary>
    /// Creates a new attachment for a plaintiff.
    /// Validates PDF only, max 4MB (BR04).
    /// </summary>
    [HttpPost("plaintiffs/{plaintiffId}/attachments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAttachment(
        [FromRoute] int plaintiffId,
        [FromBody] PlaintiffAttachmentCreateDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var attachment = await _attachmentBL.CreateAsync(plaintiffId, dto, cancellationToken);
            _logger.LogInformation("Attachment created with ID: {AttachmentId} for plaintiff {PlaintiffId}",
                attachment.Id, plaintiffId);
            return CreatedAtAction(nameof(GetAttachmentById),
                new { plaintiffId, attachmentId = attachment.Id }, attachment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create attachment: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating attachment for plaintiff {PlaintiffId}", plaintiffId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء إنشاء المرفق" });
        }
    }

    /// <summary>
    /// Deletes an attachment (soft delete).
    /// </summary>
    [HttpDelete("plaintiffs/{plaintiffId}/attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAttachment(
        [FromRoute] int plaintiffId,
        [FromRoute] int attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify attachment belongs to plaintiff
            var attachment = await _attachmentBL.GetByIdAsync(attachmentId, cancellationToken);
            if (attachment == null || attachment.PlaintiffId != plaintiffId)
                return NotFound(new { message = $"المرفق رقم {attachmentId} غير موجود" });

            await _attachmentBL.DeleteAsync(attachmentId, cancellationToken);
            _logger.LogInformation("Attachment {AttachmentId} deleted from plaintiff {PlaintiffId}",
                attachmentId, plaintiffId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete attachment: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء حذف المرفق" });
        }
    }

    /// <summary>
    /// Downloads an attachment file.
    /// </summary>
    [HttpGet("plaintiffs/{plaintiffId}/attachments/{attachmentId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadAttachment(
        [FromRoute] int plaintiffId,
        [FromRoute] int attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify attachment belongs to plaintiff
            var attachment = await _attachmentBL.GetByIdAsync(attachmentId, cancellationToken);
            if (attachment == null || attachment.PlaintiffId != plaintiffId)
                return NotFound(new { message = $"المرفق رقم {attachmentId} غير موجود" });

            var file = await _attachmentBL.GetFileAsync(attachmentId, cancellationToken);
            if (file == null)
                return NotFound(new { message = "ملف المرفق غير موجود" });

            return File(file.Value.Content, file.Value.ContentType, file.Value.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تحميل المرفق" });
        }
    }

    /// <summary>
    /// Checks if plaintiff has all required attachments.
    /// </summary>
    [HttpGet("plaintiffs/{plaintiffId}/attachments/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ValidateRequiredAttachments(
        [FromRoute] int plaintiffId,
        CancellationToken cancellationToken)
    {
        try
        {
            var hasRequired = await _attachmentBL.HasRequiredAttachmentsAsync(plaintiffId, cancellationToken);
            return Ok(new { hasRequiredAttachments = hasRequired });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating attachments for plaintiff {PlaintiffId}", plaintiffId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء التحقق من المرفقات المطلوبة" });
        }
    }

    /// <summary>
    /// Gets required attachment types for a plaintiff type.
    /// </summary>
    [HttpGet("plaintiff-types/{plaintiffTypeId}/required-attachments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetRequiredAttachmentTypes([FromRoute] int plaintiffTypeId)
    {
        var requiredTypes = _attachmentBL.GetRequiredAttachmentTypes(plaintiffTypeId);
        return Ok(new { plaintiffTypeId, requiredAttachmentTypes = requiredTypes });
    }

    #endregion
}
