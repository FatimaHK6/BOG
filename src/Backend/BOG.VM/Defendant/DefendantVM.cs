namespace BOG.VM.Defendant;

/// <summary>
/// View model for a single defendant (detailed view).
/// Used in GET /api/case-requests/defendants/{defendantId}
/// </summary>
public class DefendantVM
{
    /// <summary>
    /// Defendant ID (unique identifier)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Case registration request ID this defendant belongs to
    /// </summary>
    public int RequestId { get; set; }

    /// <summary>
    /// Defendant type ID
    /// </summary>
    public int DefendantTypeId { get; set; }

    /// <summary>
    /// Defendant type name in Arabic
    /// </summary>
    public string DefendantTypeName { get; set; } = "";

    /// <summary>
    /// Full name of the defendant
    /// </summary>
    public string FullName { get; set; } = "";

    /// <summary>
    /// Identity type ID (if available)
    /// </summary>
    public int? IdentityTypeId { get; set; }

    /// <summary>
    /// Identity type name in Arabic (if available)
    /// </summary>
    public string? IdentityTypeName { get; set; }

    /// <summary>
    /// Identity number (masked for display if sensitive)
    /// </summary>
    public string? IdentityNumber { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    public string? AddressText { get; set; }

    /// <summary>
    /// Commercial registration number (for organizations)
    /// </summary>
    public string? CommercialRegNumber { get; set; }

    /// <summary>
    /// Government agency ID (if government defendant)
    /// </summary>
    public int? GovernmentAgencyId { get; set; }

    /// <summary>
    /// Government agency name (if available)
    /// </summary>
    public string? GovernmentAgencyName { get; set; }

    /// <summary>
    /// Additional statement/notes
    /// </summary>
    public string? AdditionalStatement { get; set; }

    /// <summary>
    /// Is the defendant active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date the defendant was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date the defendant was last modified
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
