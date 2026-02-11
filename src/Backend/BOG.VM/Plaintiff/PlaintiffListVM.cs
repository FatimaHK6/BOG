namespace BOG.VM.Plaintiff;

/// <summary>
/// Plaintiff ViewModel for list/summary presentation.
/// </summary>
public class PlaintiffListVM
{
    /// <summary>
    /// Plaintiff ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Plaintiff type ID.
    /// </summary>
    public int PlaintiffTypeId { get; set; }

    /// <summary>
    /// Plaintiff type name (Arabic).
    /// </summary>
    public string PlaintiffTypeNameAr { get; set; } = null!;

    /// <summary>
    /// Full name or company name.
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Identity number.
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Mobile number.
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// Whether this plaintiff is the applicant.
    /// </summary>
    public bool IsApplicant { get; set; }

    /// <summary>
    /// Data source ID (1=FromAbsher, 2=FromUser).
    /// </summary>
    public int? DataSourceId { get; set; }

    /// <summary>
    /// Data source name.
    /// </summary>
    public string? DataSourceName { get; set; }

    /// <summary>
    /// Number of representatives.
    /// </summary>
    public int RepresentativesCount { get; set; }

    /// <summary>
    /// Number of attachments.
    /// </summary>
    public int AttachmentsCount { get; set; }

    /// <summary>
    /// Applicant badge display (computed).
    /// </summary>
    public string? ApplicantBadge => IsApplicant ? "مقدم الطلب" : null;
}
