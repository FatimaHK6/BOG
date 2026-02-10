using BOG.DAL.Interfaces;
using BOG.DbModel.Entities.Lookups;
using BOG.DbModel.Entities.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// Lookup data management API controller.
/// Provides endpoints to retrieve lookup/master data for the application.
/// </summary>
[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    private readonly IRepository<Classification> _classificationRepository;
    private readonly IRepository<AttachmentType> _attachmentTypeRepository;
    private readonly IRepository<NotificationMethod> _notificationMethodRepository;
    private readonly IRepository<GovernmentEntity> _governmentEntityRepository;
    private readonly IRepository<Court> _courtRepository;
    private readonly IRepository<CaseType> _caseTypeRepository;
    private readonly ILogger<LookupsController> _logger;

    public LookupsController(
        IRepository<Classification> classificationRepository,
        IRepository<AttachmentType> attachmentTypeRepository,
        IRepository<NotificationMethod> notificationMethodRepository,
        IRepository<GovernmentEntity> governmentEntityRepository,
        IRepository<Court> courtRepository,
        IRepository<CaseType> caseTypeRepository,
        ILogger<LookupsController> logger)
    {
        _classificationRepository = classificationRepository ?? throw new ArgumentNullException(nameof(classificationRepository));
        _attachmentTypeRepository = attachmentTypeRepository ?? throw new ArgumentNullException(nameof(attachmentTypeRepository));
        _notificationMethodRepository = notificationMethodRepository ?? throw new ArgumentNullException(nameof(notificationMethodRepository));
        _governmentEntityRepository = governmentEntityRepository ?? throw new ArgumentNullException(nameof(governmentEntityRepository));
        _courtRepository = courtRepository ?? throw new ArgumentNullException(nameof(courtRepository));
        _caseTypeRepository = caseTypeRepository ?? throw new ArgumentNullException(nameof(caseTypeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active classifications.
    /// Used by the case registration form to populate the classifications dropdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active classifications</returns>
    [HttpGet("classifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetClassifications(CancellationToken cancellationToken)
    {
        try
        {
            var classifications = await _classificationRepository.FindAsync(
                c => c.IsActive && !c.IsDeleted,
                cancellationToken);

            var result = classifications
                .Select(c => new
                {
                    id = c.Id,
                    nameAr = c.NameAr,
                    nameEn = c.Name,
                    description = c.Description,
                    level1 = c.Level1 ?? string.Empty,
                    level2 = c.Level2 ?? string.Empty,
                    level3 = c.Level3 ?? string.Empty,
                    level4 = c.Level4 ?? string.Empty
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} classifications", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classifications");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving classifications." });
        }
    }

    /// <summary>
    /// Gets all active attachment types.
    /// Used by the attachments component to populate the attachment type dropdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active attachment types</returns>
    [HttpGet("attachment-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetAttachmentTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _attachmentTypeRepository.FindAsync(
                t => t.IsActive && !t.IsDeleted,
                cancellationToken);

            var result = types
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    nameAr = t.NameAr,
                    isMandatory = t.IsMandatory,
                    description = t.Description
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} attachment types", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachment types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving attachment types." });
        }
    }

    /// <summary>
    /// Gets all active notification methods.
    /// Used in the Additional Info section (Type 1: Management Decision) for the notification method dropdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active notification methods</returns>
    [HttpGet("notification-methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetNotificationMethods(CancellationToken cancellationToken)
    {
        try
        {
            var methods = await _notificationMethodRepository.FindAsync(
                m => m.IsActive && !m.IsDeleted,
                cancellationToken);

            var result = methods
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    nameAr = m.NameAr,
                    description = m.Description
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} notification methods", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification methods");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving notification methods." });
        }
    }

    /// <summary>
    /// Gets all active government entities.
    /// Used in the Additional Info section for:
    /// - Type 1: Decision Issuing Authority dropdown
    /// - Type 2: Authority Complained To dropdown
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active government entities</returns>
    [HttpGet("government-entities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetGovernmentEntities(CancellationToken cancellationToken)
    {
        try
        {
            var entities = await _governmentEntityRepository.FindAsync(
                e => e.IsActive && !e.IsDeleted,
                cancellationToken);

            var result = entities
                .Select(e => new
                {
                    id = e.Id,
                    name = e.Name,
                    nameAr = e.NameAr,
                    code = e.Code,
                    description = e.Description
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} government entities", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving government entities");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving government entities." });
        }
    }

    /// <summary>
    /// Gets all active courts.
    /// Used in the Related Cases section for the court dropdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active courts</returns>
    [HttpGet("courts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetCourts(CancellationToken cancellationToken)
    {
        try
        {
            var courts = await _courtRepository.GetAllAsync(cancellationToken);

            var result = courts
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Id)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    nameAr = c.NameAr,
                    regionId = c.RegionId,
                    cityId = c.CityId
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} courts", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving courts." });
        }
    }

    /// <summary>
    /// Gets all active case types (إداري, تأديبي).
    /// Used in the Request Completion tab for the case type dropdown.
    /// ALWAYS REQUIRED for all decision types (Register, SendToJudge, Reject, RequestCompletion).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active case types</returns>
    [HttpGet("case-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<object>>> GetCaseTypes(CancellationToken cancellationToken)
    {
        try
        {
            var caseTypes = await _caseTypeRepository.FindAsync(
                ct => ct.IsActive && !ct.IsDeleted,
                cancellationToken);

            var result = caseTypes
                .OrderBy(ct => ct.Id)
                .Select(ct => new
                {
                    id = ct.Id,
                    name = ct.Name,
                    nameAr = ct.NameAr,
                    description = ct.Description
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} case types", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving case types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء تحميل أنواع الدعاوى" });
        }
    }
}
