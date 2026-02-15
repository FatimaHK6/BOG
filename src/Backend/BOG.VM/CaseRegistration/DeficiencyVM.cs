namespace BOG.VM.CaseRegistration;

/// <summary>
/// View Model for RequestDeficiency entity (نقص في الدعوى).
/// Used for API responses showing deficiency details with joined data.
/// </summary>
public class DeficiencyVM
{
    /// <summary>
    /// Unique identifier for the deficiency record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Foreign key to DeficiencyDescription.
    /// </summary>
    public int DeficiencyDescriptionId { get; set; }

    /// <summary>
    /// Deficiency type ID (e.g., 1 for CaseSubject, 2 for CaseClaims).
    /// Included for client-side filtering/grouping.
    /// </summary>
    public int DeficiencyTypeId { get; set; }

    /// <summary>
    /// Deficiency type name in English (e.g., "CaseSubject").
    /// </summary>
    public string DeficiencyTypeName { get; set; } = "";

    /// <summary>
    /// Deficiency type name in Arabic (e.g., "موضوع الدعوى").
    /// </summary>
    public string DeficiencyTypeNameAr { get; set; } = "";

    /// <summary>
    /// Deficiency description in Arabic (e.g., "موضوع الدعوى غير واضح").
    /// </summary>
    public string DescriptionAr { get; set; } = "";

    /// <summary>
    /// Deficiency description in English (optional).
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Display order for sorting in the UI.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Date and time when the deficiency was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date and time when the deficiency was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
