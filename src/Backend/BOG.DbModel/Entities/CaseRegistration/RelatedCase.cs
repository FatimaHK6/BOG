using BOG.DbModel.Entities.Identity;

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
    /// Related case number (رقم القضية المرتبطة).
    /// </summary>
    public int CaseNumber { get; set; }

    /// <summary>
    /// Case year for the related case.
    /// </summary>
    public int CaseYear { get; set; }

    /// <summary>
    /// Foreign key to Court.
    /// </summary>
    public int? CourtId { get; set; }

    /// <summary>
    /// Navigation property for the request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;

    /// <summary>
    /// Navigation property for the court.
    /// </summary>
    public virtual Court? Court { get; set; }
}
