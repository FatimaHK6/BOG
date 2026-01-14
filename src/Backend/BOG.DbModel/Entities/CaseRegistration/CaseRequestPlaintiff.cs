namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Junction table for many-to-many relationship between CaseRegistrationRequest and Plaintiff.
/// </summary>
public class CaseRequestPlaintiff : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Foreign key to Plaintiff.
    /// </summary>
    public int PlaintiffId { get; set; }

    /// <summary>
    /// Navigation property for request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;

    /// <summary>
    /// Navigation property for plaintiff.
    /// </summary>
    public virtual Plaintiff Plaintiff { get; set; } = null!;
}
