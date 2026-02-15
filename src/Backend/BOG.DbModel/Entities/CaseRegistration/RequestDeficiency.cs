namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for case registration request deficiencies (نواقص الدعوى) - one-to-many with CaseRegistrationRequest.
/// Each deficiency is a reference to a predefined DeficiencyDescription.
/// </summary>
public class RequestDeficiency : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Foreign key to DeficiencyDescription (predefined deficiency template).
    /// </summary>
    public int DeficiencyDescriptionId { get; set; }

    /// <summary>
    /// Display order for sorting deficiencies in the list.
    /// </summary>
    public int DisplayOrder { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property for the request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;

    /// <summary>
    /// Navigation property for the deficiency description.
    /// </summary>
    public virtual Lookups.DeficiencyDescription DeficiencyDescription { get; set; } = null!;

    #endregion
}
