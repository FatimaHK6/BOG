namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for case claims (الطلبات) - one-to-many with CaseRegistrationRequest.
/// </summary>
public class Claim : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Claim text (نص الطلب) - max 2000 characters.
    /// </summary>
    public string ClaimText { get; set; } = null!;

    /// <summary>
    /// Display order for sorting claims.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Navigation property for the request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;
}
