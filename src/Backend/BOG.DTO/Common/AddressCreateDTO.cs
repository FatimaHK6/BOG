using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Common;

/// <summary>
/// DTO for creating/updating a national address (العنوان الوطني).
/// Based on SRS 6.3.2.
/// BC04: Only Region and City are required, other fields are optional.
/// </summary>
public class AddressCreateDTO
{
    /// <summary>
    /// Region ID (المنطقة) - from 13 administrative regions.
    /// Required per BC04.
    /// </summary>
    [Required(ErrorMessage = "المنطقة مطلوبة")]
    public int RegionId { get; set; }

    /// <summary>
    /// City ID (المدينة) - from cities of selected region.
    /// Required per BC04.
    /// </summary>
    [Required(ErrorMessage = "المدينة مطلوبة")]
    public int CityId { get; set; }

    /// <summary>
    /// District ID (الحي) - from districts of selected city.
    /// Optional per BC04.
    /// </summary>
    public int? DistrictId { get; set; }

    /// <summary>
    /// Street name (الشارع) - max 200 characters.
    /// Optional per BC04.
    /// </summary>
    [StringLength(200, ErrorMessage = "الشارع يجب ألا يتجاوز 200 حرف")]
    public string? Street { get; set; }

    /// <summary>
    /// Building number (رقم المبنى) - exactly 4 digits.
    /// Optional per BC04.
    /// </summary>
    [StringLength(4, ErrorMessage = "رقم المبنى يجب أن يكون 4 أرقام")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "رقم المبنى يجب أن يكون 4 أرقام فقط")]
    public string? BuildingNumber { get; set; }

    /// <summary>
    /// Unit number (رقم الوحدة) - digits only.
    /// Optional per BC04.
    /// </summary>
    [RegularExpression(@"^\d+$", ErrorMessage = "رقم الوحدة يجب أن يكون أرقام فقط")]
    public string? UnitNumber { get; set; }

    /// <summary>
    /// Postal code (الرمز البريدي) - exactly 5 digits.
    /// Optional per BC04.
    /// </summary>
    [StringLength(5, ErrorMessage = "الرمز البريدي يجب أن يكون 5 أرقام")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "الرمز البريدي يجب أن يكون 5 أرقام فقط")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Additional code (الرمز الإضافي) - exactly 4 digits.
    /// Optional per BC04.
    /// </summary>
    [StringLength(4, ErrorMessage = "الرمز الإضافي يجب أن يكون 4 أرقام")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "الرمز الإضافي يجب أن يكون 4 أرقام فقط")]
    public string? AdditionalCode { get; set; }
}
