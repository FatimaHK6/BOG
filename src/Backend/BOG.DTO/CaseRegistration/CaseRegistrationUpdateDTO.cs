using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// Data transfer object for updating a case registration request.
/// Used in PUT /api/case-requests/{requestId}
/// All fields are optional - only provided fields will be updated
/// Can only update requests in Draft or PendingCompletion state
/// </summary>
public class CaseRegistrationUpdateDTO
{
    /// <summary>
    /// Case subject/title (optional)
    /// </summary>
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Subject must be between 10 and 4000 characters")]
    public string? Subject { get; set; }

    /// <summary>
    /// Evidence description (optional)
    /// </summary>
    [StringLength(4000, MinimumLength = 20, ErrorMessage = "Evidence must be between 20 and 4000 characters")]
    public string? Evidence { get; set; }

    /// <summary>
    /// Court ID (optional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Invalid court ID")]
    public int? CourtId { get; set; }

    /// <summary>
    /// Case type ID (optional)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Invalid case type")]
    public int? CaseTypeId { get; set; }

    /// <summary>
    /// Additional case notes (optional)
    /// </summary>
    [StringLength(4000, ErrorMessage = "Notes cannot exceed 4000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// Classification IDs for the case (optional)
    /// </summary>
    public List<int>? ClassificationIds { get; set; }

    /// <summary>
    /// Primary mobile number for contact (optional)
    /// </summary>
    public string? PrimaryMobile { get; set; }

    /// <summary>
    /// Secondary mobile number for contact (optional)
    /// </summary>
    public string? SecondaryMobile { get; set; }

    /// <summary>
    /// Email address for contact (optional)
    /// </summary>
    public string? Email { get; set; }
}
