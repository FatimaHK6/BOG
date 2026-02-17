namespace BOG.DbModel.Constants;

/// <summary>
/// Constants for PlaintiffType lookup table IDs.
/// Used to determine plaintiff classification and validation rules (e.g., corporate plaintiffs requiring representatives).
/// </summary>
public static class PlaintiffTypeIds
{
    /// <summary>
    /// Individual person (فرد) - Does NOT require representatives.
    /// </summary>
    public const int Individual = 1;

    /// <summary>
    /// Individual without ID (فرد بدون هوية) - Does NOT require representatives.
    /// </summary>
    public const int IndividualWithoutId = 2;

    /// <summary>
    /// Business Owner (صاحب مؤسسة) - CORPORATE - REQUIRES representatives.
    /// </summary>
    public const int BusinessOwner = 3;

    /// <summary>
    /// Registered Company (شركة مسجلة) - CORPORATE - REQUIRES representatives.
    /// </summary>
    public const int RegisteredCompany = 4;

    /// <summary>
    /// Unregistered Company (شركة غير مسجلة) - CORPORATE - REQUIRES representatives.
    /// </summary>
    public const int UnregisteredCompany = 5;

    /// <summary>
    /// Government Agency (جهة حكومية) - CORPORATE - REQUIRES representatives.
    /// </summary>
    public const int GovernmentAgency = 6;

    /// <summary>
    /// NGO / Civil Society Organization (جمعية/مؤسسة أهلية) - CORPORATE - REQUIRES representatives.
    /// </summary>
    public const int Ngo = 7;

    /// <summary>
    /// Waqf (وقف) - CORPORATE - REQUIRES representatives.
    /// </summary>
    public const int Waqf = 8;

    /// <summary>
    /// Returns true if the plaintiff type is a corporate entity that requires a representative.
    /// Corporate types: 3, 4, 5, 6, 7, 8
    /// </summary>
    public static bool IsCorporate(int plaintiffTypeId) =>
        plaintiffTypeId >= BusinessOwner && plaintiffTypeId <= Waqf;

    /// <summary>
    /// Corporate plaintiff type IDs (types 3-8) that require representatives.
    /// </summary>
    public static readonly int[] CorporateTypeIds = {
        BusinessOwner,
        RegisteredCompany,
        UnregisteredCompany,
        GovernmentAgency,
        Ngo,
        Waqf
    };
}
