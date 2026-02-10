using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration.Defendant;

/// <summary>
/// Data transfer object for creating a new defendant.
/// Used in POST /api/case-requests/{requestId}/defendants
/// </summary>
public class DefendantCreateDTO
{
    /// <summary>
    /// Defendant type ID (1-6: Individual, Organization, Government Agency, Representative, etc.)
    /// </summary>
    [Required(ErrorMessage = "Defendant type is required")]
    [Range(1, 10, ErrorMessage = "Invalid defendant type")]
    public int DefendantTypeId { get; set; }

    /// <summary>
    /// Full name of the defendant (required)
    /// </summary>
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 200 characters")]
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Identity type ID (optional, 1-4: National ID, Passport, Commercial Register, etc.)
    /// </summary>
    [Range(1, 10, ErrorMessage = "Invalid identity type")]
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity number (optional, max 50 chars, validated by identity type)
    /// </summary>
    [StringLength(50, ErrorMessage = "Identity number cannot exceed 50 characters")]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Street address (optional)
    /// </summary>
    [StringLength(2000, ErrorMessage = "Address cannot exceed 2000 characters")]
    public string? AddressText { get; set; }

    /// <summary>
    /// Commercial registration number for organizations (optional)
    /// </summary>
    [StringLength(50, ErrorMessage = "Commercial registration number cannot exceed 50 characters")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Commercial registration number must be 10 digits")]
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Government agency ID (foreign key, optional for government defendants)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Invalid government agency")]
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Additional notes about defendant (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "Additional statement cannot exceed 4000 characters")]
    public string? AdditionalStatement { get; set; }
}
