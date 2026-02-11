namespace BOG.DbModel.Entities.Lookups;

/// <summary>
/// Lookup entity for countries (الدولة) - SRS 6.3.7.
/// Used for unregistered foreign companies.
/// </summary>
public class Country : BaseEntity
{
    /// <summary>
    /// Country name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Country name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// ISO country code (e.g., SA, AE, EG).
    /// </summary>
    public string? IsoCode { get; set; }

    /// <summary>
    /// Phone country code (e.g., 966, 971).
    /// </summary>
    public string? PhoneCode { get; set; }

    /// <summary>
    /// Whether the country is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
