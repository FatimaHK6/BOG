using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// Data transfer object for creating a new case registration request.
/// Used in POST /api/case-requests
/// Creates request in Draft state
/// </summary>
public class CaseRegistrationCreateDTO
{
    /// <summary>
    /// Court ID where case will be registered (required)
    /// </summary>
    [Required(ErrorMessage = "Court ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid court ID")]
    public int CourtId { get; set; }

    /// <summary>
    /// Case subject/title (required)
    /// </summary>
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Subject must be between 10 and 4000 characters")]
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Evidence description (required)
    /// </summary>
    [Required(ErrorMessage = "Evidence is required")]
    [StringLength(4000, MinimumLength = 20, ErrorMessage = "Evidence must be between 20 and 4000 characters")]
    public string Evidence { get; set; } = null!;

    /// <summary>
    /// Case Type ID (إداري or تأديبي) - Default: 1 (إداري)
    /// </summary>
    [Range(1, 2, ErrorMessage = "Invalid case type")]
    public int CaseTypeId { get; set; } = 1;

    /// <summary>
    /// Submission method ID (Through Court or Through Portal)
    /// Optional - defaults to "Through Court" if not provided
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Invalid applying method")]
    public int? ApplyingMethodId { get; set; }

    /// <summary>
    /// Additional case notes (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "Notes cannot exceed 4000 characters")]
    public string? Notes { get; set; }
}
