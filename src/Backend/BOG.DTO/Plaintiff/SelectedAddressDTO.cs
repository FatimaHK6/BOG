using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Plaintiff;

/// <summary>
/// DTO for setting the selected address for notifications (العنوان المختار).
/// </summary>
public class SelectedAddressDTO
{
    /// <summary>
    /// Address source type: "Residence" or "Work".
    /// </summary>
    [Required(ErrorMessage = "نوع العنوان مطلوب")]
    [RegularExpression(@"^(Residence|Work)$", ErrorMessage = "نوع العنوان يجب أن يكون 'Residence' أو 'Work'")]
    public string AddressSourceType { get; set; } = null!;

    /// <summary>
    /// If a custom address is provided instead of existing addresses.
    /// </summary>
    public AddressDTO? CustomAddress { get; set; }
}

/// <summary>
/// DTO for address data.
/// </summary>
public class AddressDTO
{
    /// <summary>
    /// Building number (رقم المبنى).
    /// </summary>
    [StringLength(4)]
    public string? BuildingNumber { get; set; }

    /// <summary>
    /// Street name (اسم الشارع).
    /// </summary>
    [StringLength(200)]
    public string? StreetName { get; set; }

    /// <summary>
    /// District/Neighborhood (الحي).
    /// </summary>
    [StringLength(100)]
    public string? District { get; set; }

    /// <summary>
    /// City name (المدينة).
    /// </summary>
    [StringLength(100)]
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
    /// Postal code (الرمز البريدي) - 5 digits.
    /// </summary>
    [StringLength(5)]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "الرمز البريدي يجب أن يكون 5 أرقام")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Additional number (الرقم الإضافي) - 4 digits.
    /// </summary>
    [StringLength(4)]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "الرقم الإضافي يجب أن يكون 4 أرقام")]
    public string? AdditionalNumber { get; set; }

    /// <summary>
    /// Unit number (رقم الوحدة).
    /// </summary>
    [StringLength(10)]
    public string? UnitNumber { get; set; }
}
