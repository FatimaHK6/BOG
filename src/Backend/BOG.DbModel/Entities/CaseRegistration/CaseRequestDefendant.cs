namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Junction table for many-to-many relationship between CaseRegistrationRequest and Defendant.
/// </summary>
public class CaseRequestDefendant : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Foreign key to Defendant.
    /// </summary>
    public int DefendantId { get; set; }

    /// <summary>
    /// Navigation property for request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;

    /// <summary>
    /// Navigation property for defendant.
    /// </summary>
    public virtual Defendant Defendant { get; set; } = null!;
}
