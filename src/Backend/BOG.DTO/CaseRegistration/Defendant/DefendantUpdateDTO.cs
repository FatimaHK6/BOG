using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration.Defendant;

/// <summary>
/// Data transfer object for updating an existing defendant.
/// Used in PUT /api/case-requests/defendants/{defendantId}
/// All fields are optional - only provided fields will be updated
/// </summary>
public class DefendantUpdateDTO
{
    /// <summary>
    /// Full name of the defendant (optional)
    /// </summary>
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 200 characters")]
    public string? FullName { get; set; }

    /// <summary>
    /// Street address (optional)
    /// </summary>
    [StringLength(2000, ErrorMessage = "Address cannot exceed 2000 characters")]
    public string? AddressText { get; set; }

    /// <summary>
    /// Additional notes about defendant (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "Additional statement cannot exceed 4000 characters")]
    public string? AdditionalStatement { get; set; }

    /// <summary>
    /// Identity number (optional)
    /// </summary>
    [StringLength(50, ErrorMessage = "Identity number cannot exceed 50 characters")]
    public string? IdentityNumber { get; set; }
}
