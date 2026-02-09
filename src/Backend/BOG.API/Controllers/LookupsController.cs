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
}
