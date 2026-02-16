using BOG.DbModel;
using BOG.DbModel.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BOG.API.Controllers;

/// <summary>
/// Lookups API controller for dropdown data.
/// </summary>
[ApiController]
[Route("api/lookups")]
public class LookupsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LookupsController> _logger;

    public LookupsController(ApplicationDbContext context, ILogger<LookupsController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active government agencies.
    /// </summary>
    [HttpGet("government-agencies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetGovernmentAgencies(CancellationToken cancellationToken)
    {
        try
        {
            var agencies = await _context.GovernmentAgencies
                .AsNoTracking()
                .Where(a => a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.NameAr)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.NameAr,
                    a.Code
                })
                .ToListAsync(cancellationToken);

            return Ok(agencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving government agencies");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع الجهات الحكومية" });
        }
    }

    /// <summary>
    /// Gets all active defendant types.
    /// </summary>
    [HttpGet("defendant-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDefendantTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _context.DefendantTypes
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameAr
                })
                .ToListAsync(cancellationToken);

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving defendant types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع أنواع المدعى عليهم" });
        }
    }

    /// <summary>
    /// Gets all nationalities (from enum).
    /// </summary>
    [HttpGet("nationalities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetNationalities()
    {
        var nationalities = Enum.GetValues<Nationality>()
            .Select(n => new
            {
                Id = (int)n,
                Name = n.ToString(),
                NameAr = GetNationalityArabicName(n)
            })
            .OrderBy(n => n.Id == 1 ? 0 : 1) // Saudi first
            .ThenBy(n => n.NameAr)
            .ToList();

        return Ok(nationalities);
    }

    private static string GetNationalityArabicName(Nationality nationality)
    {
        return nationality switch
        {
            Nationality.Saudi => "سعودي",
            Nationality.Emirati => "إماراتي",
            Nationality.Kuwaiti => "كويتي",
            Nationality.Bahraini => "بحريني",
            Nationality.Qatari => "قطري",
            Nationality.Omani => "عماني",
            Nationality.Egyptian => "مصري",
            Nationality.Jordanian => "أردني",
            Nationality.Lebanese => "لبناني",
            Nationality.Syrian => "سوري",
            Nationality.Iraqi => "عراقي",
            Nationality.Yemeni => "يمني",
            Nationality.Palestinian => "فلسطيني",
            Nationality.Sudanese => "سوداني",
            Nationality.Tunisian => "تونسي",
            Nationality.Moroccan => "مغربي",
            Nationality.Algerian => "جزائري",
            Nationality.Libyan => "ليبي",
            Nationality.Indian => "هندي",
            Nationality.Pakistani => "باكستاني",
            Nationality.Bangladeshi => "بنغلاديشي",
            Nationality.Filipino => "فلبيني",
            Nationality.Indonesian => "إندونيسي",
            Nationality.American => "أمريكي",
            Nationality.British => "بريطاني",
            Nationality.French => "فرنسي",
            Nationality.German => "ألماني",
            Nationality.Other => "أخرى",
            _ => nationality.ToString()
        };
    }

    /// <summary>
    /// Gets all active regions.
    /// </summary>
    [HttpGet("regions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRegions(CancellationToken cancellationToken)
    {
        try
        {
            var regions = await _context.Regions
                .AsNoTracking()
                .Where(r => r.IsActive && !r.IsDeleted)
                .OrderBy(r => r.NameAr)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.NameAr,
                    r.Code
                })
                .ToListAsync(cancellationToken);

            return Ok(regions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving regions");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع المناطق" });
        }
    }

    /// <summary>
    /// Gets cities by region ID.
    /// </summary>
    [HttpGet("regions/{regionId}/cities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetCitiesByRegion([FromRoute] int regionId, CancellationToken cancellationToken)
    {
        try
        {
            var cities = await _context.Cities
                .AsNoTracking()
                .Where(c => c.RegionId == regionId && c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.NameAr)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.NameAr,
                    c.Code,
                    c.RegionId
                })
                .ToListAsync(cancellationToken);

            return Ok(cities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cities for region {RegionId}", regionId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع المدن" });
        }
    }

    /// <summary>
    /// Gets all active plaintiff types.
    /// </summary>
    [HttpGet("plaintiff-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPlaintiffTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _context.PlaintiffTypes
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameAr
                })
                .ToListAsync(cancellationToken);

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plaintiff types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع أنواع المدعين" });
        }
    }

    /// <summary>
    /// Gets all active identity types.
    /// </summary>
    [HttpGet("identity-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetIdentityTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _context.IdentityTypes
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameAr
                })
                .ToListAsync(cancellationToken);

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving identity types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع أنواع الهوية" });
        }
    }

    /// <summary>
    /// Gets all active representative types.
    /// </summary>
    [HttpGet("representative-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRepresentativeTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _context.RepresentativeTypes
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameAr
                })
                .ToListAsync(cancellationToken);

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving representative types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع أنواع الممثلين" });
        }
    }

    /// <summary>
    /// Gets all active countries.
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetCountries(CancellationToken cancellationToken)
    {
        try
        {
            var countries = await _context.Countries
                .AsNoTracking()
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.NameAr)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.NameAr,
                    c.IsoCode
                })
                .ToListAsync(cancellationToken);

            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving countries");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع الدول" });
        }
    }

    /// <summary>
    /// Gets all active classifications.
    /// </summary>
    [HttpGet("classifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetClassifications(CancellationToken cancellationToken)
    {
        try
        {
            var classifications = await _context.Classifications
                .AsNoTracking()
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.NameAr)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.NameAr,
                    c.Description,
                    c.Level1,
                    c.Level2,
                    c.Level3,
                    c.Level4,
                    c.IsActive
                })
                .ToListAsync(cancellationToken);

            return Ok(classifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classifications");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع التصنيفات" });
        }
    }

    /// <summary>
    /// Gets all active deficiency types.
    /// </summary>
    [HttpGet("deficiency-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDeficiencyTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _context.DeficiencyTypes
                .AsNoTracking()
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.DisplayOrder)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameAr,
                    t.DisplayOrder
                })
                .ToListAsync(cancellationToken);

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deficiency types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع أنواع النواقص" });
        }
    }

    /// <summary>
    /// Gets all active deficiency descriptions, optionally filtered by deficiency type.
    /// </summary>
    [HttpGet("deficiency-descriptions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetDeficiencyDescriptions([FromQuery] int? typeId, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.DeficiencyDescriptions
                .AsNoTracking()
                .Where(d => !d.IsDeleted);

            if (typeId.HasValue)
            {
                query = query.Where(d => d.DeficiencyTypeId == typeId.Value);
            }

            var descriptions = await query
                .OrderBy(d => d.DeficiencyTypeId)
                .ThenBy(d => d.DisplayOrder)
                .Select(d => new
                {
                    d.Id,
                    d.DeficiencyTypeId,
                    d.DescriptionAr,
                    d.DescriptionEn,
                    d.DisplayOrder
                })
                .ToListAsync(cancellationToken);

            return Ok(descriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deficiency descriptions");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع تفاصيل النواقص" });
        }
    }

    /// <summary>
    /// Gets all active case types.
    /// </summary>
    /// <returns>List of active case types</returns>
    [HttpGet("case-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetCaseTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _context.CaseTypes
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameAr,
                    t.Description
                })
                .ToListAsync(cancellationToken);

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving case types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع أنواع الدعاوى" });
        }
    }

    /// <summary>
    /// Gets all active courts.
    /// </summary>
    [HttpGet("courts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetCourts(CancellationToken cancellationToken)
    {
        try
        {
            var courts = await _context.Courts
                .AsNoTracking()
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.NameAr)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.NameAr,
                    c.RegionId,
                    c.CityId
                })
                .ToListAsync(cancellationToken);

            return Ok(courts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع المحاكم" });
        }
    }

    /// <summary>
    /// Gets all active attachment types.
    /// </summary>
    [HttpGet("attachment-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAttachmentTypes(CancellationToken cancellationToken)
    {
        try
        {
            var attachmentTypes = await _context.AttachmentTypes
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.NameAr)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.NameAr,
                    t.Description,
                    t.IsMandatory,
                    t.MaxFileSizeBytes,
                    t.AllowedExtensions
                })
                .ToListAsync(cancellationToken);

            return Ok(attachmentTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachment types");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع أنواع المرفقات" });
        }
    }

    /// <summary>
    /// Gets all active notification methods.
    /// </summary>
    [HttpGet("notification-methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetNotificationMethods(CancellationToken cancellationToken)
    {
        try
        {
            var methods = await _context.NotificationMethods
                .AsNoTracking()
                .Where(m => m.IsActive && !m.IsDeleted)
                .OrderBy(m => m.NameAr)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.NameAr,
                    m.Description
                })
                .ToListAsync(cancellationToken);

            return Ok(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification methods");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع طرق الإعلام" });
        }
    }

    /// <summary>
    /// Gets all active applying methods.
    /// </summary>
    [HttpGet("applying-methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetApplyingMethods(CancellationToken cancellationToken)
    {
        try
        {
            var methods = await _context.ApplyingMethods
                .AsNoTracking()
                .Where(m => m.IsActive && !m.IsDeleted)
                .OrderBy(m => m.Id)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.NameAr
                })
                .ToListAsync(cancellationToken);

            return Ok(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applying methods");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع طرق التقديم" });
        }
    }

    /// <summary>
    /// Gets all active government entities.
    /// </summary>
    [HttpGet("government-entities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetGovernmentEntities(CancellationToken cancellationToken)
    {
        try
        {
            var entities = await _context.GovernmentEntities
                .AsNoTracking()
                .Where(e => e.IsActive && !e.IsDeleted)
                .OrderBy(e => e.NameAr)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.NameAr,
                    e.Code,
                    e.Description
                })
                .ToListAsync(cancellationToken);

            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving government entities");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ أثناء استرجاع الجهات الحكومية" });
        }
    }
}
