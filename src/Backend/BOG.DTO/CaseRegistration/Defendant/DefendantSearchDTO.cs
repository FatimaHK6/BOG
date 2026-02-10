using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.CaseRegistration.Defendant;

/// <summary>
/// Data transfer object for searching defendants.
/// Used in GET /api/case-requests/defendants/search
/// All filter fields are optional
/// </summary>
public class DefendantSearchDTO
{
    /// <summary>
    /// Filter by defendant type ID
    /// </summary>
    [Range(1, 10, ErrorMessage = "Invalid defendant type")]
    public int? DefendantTypeId { get; set; }

    /// <summary>
    /// Filter by full name (partial match, case-insensitive)
    /// </summary>
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string? FullName { get; set; }

    /// <summary>
    /// Filter by identity number
    /// </summary>
    [StringLength(50, ErrorMessage = "Identity number cannot exceed 50 characters")]
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Filter by case registration request ID
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Invalid request ID")]
    public int? RequestId { get; set; }

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
    /// Sort field (FullName, CreatedDate, etc.)
    /// </summary>
    [StringLength(50)]
    public string? SortBy { get; set; } = "CreatedDate";

    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
    public string SortDirection { get; set; } = "desc";
}
