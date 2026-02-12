using BOG.BL.Interfaces.CaseRegistration;
using BOG.DTO.RequestAttachment;
using BOG.VM.RequestAttachment;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Request attachment management API controller.
/// Handles attachment upload, retrieval, and deletion for case registration requests.
/// Enforces business rule BR04: PDF only, max 4MB per file.
/// Follows Single Responsibility Principle and Dependency Inversion Principle.
/// </summary>
[ApiController]
[Route("api/case-requests")]
public class RequestAttachmentController : ControllerBase
{
    private readonly IRequestAttachmentBL _attachmentBL;
    private readonly ILogger<RequestAttachmentController> _logger;

    public RequestAttachmentController(IRequestAttachmentBL attachmentBL, ILogger<RequestAttachmentController> logger)
    {
        _attachmentBL = attachmentBL ?? throw new ArgumentNullException(nameof(attachmentBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all attachments for a case registration request.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of attachments for the request</returns>
    [HttpGet("{requestId}/attachments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RequestAttachmentVM>>> GetAttachments(
        [FromRoute] int requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var attachments = await _attachmentBL.GetAttachmentsByRequestIdAsync(requestId, cancellationToken);
            return Ok(attachments);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Request not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving attachments." });
        }
    }

    /// <summary>
    /// Uploads a new attachment to a case registration request.
    /// Validates BR04: PDF only, max 4MB per file.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="uploadDto">The attachment data (file content, type, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created attachment</returns>
    [HttpPost("{requestId}/attachments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestAttachmentVM>> UploadAttachment(
        [FromRoute] int requestId,
        [FromBody] RequestAttachmentDTO uploadDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            var attachment = await _attachmentBL.AddAttachmentAsync(requestId, uploadDto, cancellationToken);
            _logger.LogInformation("Attachment uploaded for request {RequestId}, AttachmentId: {AttachmentId}",
                requestId, attachment.Id);

            return CreatedAtAction(nameof(GetAttachment),
                new { requestId, attachmentId = attachment.Id }, attachment);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("BR04"))
        {
            _logger.LogWarning("BR04 validation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message, errorCode = "BR04" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to upload attachment: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid argument: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment for request {RequestId}", requestId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while uploading the attachment." });
        }
    }

    /// <summary>
    /// Gets a specific attachment by ID.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="attachmentId">The attachment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The attachment details</returns>
    [HttpGet("{requestId}/attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequestAttachmentVM>> GetAttachment(
        [FromRoute] int requestId,
        [FromRoute] int attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            if (attachmentId <= 0)
                return BadRequest(new { message = "Invalid attachment ID." });

            var attachment = await _attachmentBL.GetAttachmentByIdAsync(attachmentId, cancellationToken);
            if (attachment == null)
                return NotFound(new { message = $"Attachment with ID {attachmentId} not found." });

            return Ok(attachment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachment {AttachmentId}", attachmentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the attachment." });
        }
    }

    /// <summary>
    /// Deletes an attachment (soft delete).
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="attachmentId">The attachment ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{requestId}/attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAttachment(
        [FromRoute] int requestId,
        [FromRoute] int attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0)
                return BadRequest(new { message = "Invalid request ID." });

            if (attachmentId <= 0)
                return BadRequest(new { message = "Invalid attachment ID." });

            await _attachmentBL.DeleteAttachmentAsync(attachmentId, cancellationToken);
            _logger.LogInformation("Attachment {AttachmentId} deleted", attachmentId);
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
                new { message = "An error occurred while deleting the attachment." });
        }
    }

    /// <summary>
    /// Downloads the attachment file.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="attachmentId">The attachment ID to download</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content with PDF mime type</returns>
    [HttpGet("{requestId}/attachments/{attachmentId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadAttachment(
        [FromRoute] int requestId,
        [FromRoute] int attachmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (requestId <= 0 || attachmentId <= 0)
                return BadRequest(new { message = "Invalid request or attachment ID." });

            var (fileName, fileContent) = await _attachmentBL.DownloadAttachmentAsync(attachmentId, cancellationToken);

            _logger.LogInformation("Attachment {AttachmentId} downloaded", attachmentId);
            return File(fileContent, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Attachment not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to download attachment: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while downloading the attachment." });
        }
    }
}
