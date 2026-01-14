namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for related cases (الدعاوى المرتبطة) - one-to-many with CaseRegistrationRequest.
/// </summary>
public class RelatedCase : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Related case number (رقم القضية المرتبطة) - max 50 characters.
    /// </summary>
    public string CaseNumber { get; set; } = null!;

    /// <summary>
    /// Notes about the relationship.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property for the request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;
}
