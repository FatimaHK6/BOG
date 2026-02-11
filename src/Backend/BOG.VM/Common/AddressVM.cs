namespace BOG.VM.Common;

/// <summary>
/// Address ViewModel for presentation layer.
/// </summary>
public class AddressVM
{
    /// <summary>
    /// Address ID.
    /// </summary>
    public int Id { get; set; }

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
    /// City ID.
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// Region ID.
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Region name in Arabic (اسم المنطقة).
    /// </summary>
    public string? RegionName { get; set; }

    /// <summary>
    /// City name in Arabic (اسم المدينة).
    /// </summary>
    public string? CityName { get; set; }

    /// <summary>
    /// District ID.
    /// </summary>
    public int? DistrictId { get; set; }

    /// <summary>
    /// District name in Arabic (اسم الحي).
    /// </summary>
    public string? DistrictName { get; set; }

    /// <summary>
    /// Postal code (الرمز البريدي).
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Additional number (الرقم الإضافي).
    /// </summary>
    public string? AdditionalNumber { get; set; }

    /// <summary>
    /// Unit number (رقم الوحدة).
    /// </summary>
    public string? UnitNumber { get; set; }

    /// <summary>
    /// Address type: Residence (سكن), Work (عمل).
    /// </summary>
    public string? AddressType { get; set; }

    /// <summary>
    /// Full formatted address (computed).
    /// </summary>
    public string FullAddress
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(BuildingNumber)) parts.Add(BuildingNumber);
            if (!string.IsNullOrEmpty(StreetName)) parts.Add(StreetName);
            if (!string.IsNullOrEmpty(District)) parts.Add(District);
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (!string.IsNullOrEmpty(PostalCode)) parts.Add(PostalCode);
            return string.Join(", ", parts);
        }
    }
}
