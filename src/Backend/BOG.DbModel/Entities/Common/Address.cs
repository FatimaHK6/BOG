using BOG.DbModel.Entities.Lookups;

namespace BOG.DbModel.Entities.Common;

/// <summary>
/// Entity for National Address (العنوان الوطني) following Saudi National Address format (6.3.2).
/// </summary>
public class Address : BaseEntity
{
    /// <summary>
    /// Building number (رقم المبنى).
    /// </summary>
    public string? BuildingNumber { get; set; }

    /// <summary>
    /// Street name (اسم الشارع).
    /// </summary>
    public string? StreetName { get; set; }

    /// <summary>
    /// District/Neighborhood (الحي).
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// City name (المدينة).
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Postal code (الرمز البريدي) - 5 digits.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Additional number (الرقم الإضافي) - 4 digits.
    /// </summary>
    public string? AdditionalNumber { get; set; }

    /// <summary>
    /// Unit number (رقم الوحدة).
    /// </summary>
    public string? UnitNumber { get; set; }

    /// <summary>
    /// Region ID (المنطقة).
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// City ID (المدينة).
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// Address type: Residence (سكن), Work (عمل), Other.
    /// </summary>
    public string? AddressType { get; set; }

    /// <summary>
    /// Full formatted address text.
    /// </summary>
    public string? FullAddress { get; set; }

    /// <summary>
    /// Whether this address is primary.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Whether the address is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for Region.
    /// </summary>
    public virtual Region? Region { get; set; }

    /// <summary>
    /// Navigation property for City.
    /// </summary>
    public virtual City? City_ { get; set; }
}
