using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration;

/// <summary>
/// Data transfer object for searching case registration requests.
/// Used in GET /api/case-requests/search
/// All filter fields are optional
/// </summary>
public class SearchRequestDTO
{
    /// <summary>
    /// Filter by request status ID
    /// </summary>
    [Range(1, 10, ErrorMessage = "Invalid status ID")]
    public int? StatusId { get; set; }

    /// <summary>
    /// Filter by court ID
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Invalid court ID")]
    public int? CourtId { get; set; }

    /// <summary>
    /// Filter by case subject (partial match, case-insensitive)
    /// </summary>
    [StringLength(4000, ErrorMessage = "Subject cannot exceed 4000 characters")]
    public string? Subject { get; set; }

    /// <summary>
    /// Filter by plaintiff identity number
    /// </summary>
    [StringLength(50, ErrorMessage = "Identity number cannot exceed 50 characters")]
    public string? PlaintiffIdentityNumber { get; set; }

    /// <summary>
    /// Filter by plaintiff name (partial match)
    /// </summary>
    [StringLength(200, ErrorMessage = "Plaintiff name cannot exceed 200 characters")]
    public string? PlaintiffName { get; set; }

    /// <summary>
    /// Filter by defendant identity number
    /// </summary>
    [StringLength(50, ErrorMessage = "Identity number cannot exceed 50 characters")]
    public string? DefendantIdentityNumber { get; set; }

    /// <summary>
    /// Filter by defendant name (partial match)
    /// </summary>
    [StringLength(200, ErrorMessage = "Defendant name cannot exceed 200 characters")]
    public string? DefendantName { get; set; }

    /// <summary>
    /// Filter by case number (if already registered)
    /// </summary>
    [StringLength(50, ErrorMessage = "Case number cannot exceed 50 characters")]
    public string? CaseNumber { get; set; }

    /// <summary>
    /// Filter by submission date (from)
    /// </summary>
    public DateTime? SubmissionDateFrom { get; set; }

    /// <summary>
    /// Filter by submission date (to)
    /// </summary>
    public DateTime? SubmissionDateTo { get; set; }

    /// <summary>
    /// Filter by registration date (from)
    /// </summary>
    public DateTime? RegistrationDateFrom { get; set; }

    /// <summary>
    /// Filter by registration date (to)
    /// </summary>
    public DateTime? RegistrationDateTo { get; set; }

    /// <summary>
    /// Page number for pagination (1-based, default 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be >= 1")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (default 10, max 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field (CreatedDate, Subject, Status, etc.)
    /// </summary>
    [StringLength(50)]
    public string SortBy { get; set; } = "CreatedDate";

    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
    public string SortDirection { get; set; } = "desc";
}
