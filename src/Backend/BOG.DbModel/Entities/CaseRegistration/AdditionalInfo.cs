namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Additional information entity for case registration requests.
/// Base table for storing type-specific additional information.
/// Supports three types: Management Decision, Service/Retirement Rights, and Trademark.
/// One-to-one relationship with CaseRegistrationRequest.
/// </summary>
public class AdditionalInfo : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to the associated case registration request.
    /// </summary>
    public virtual CaseRegistrationRequest? CaseRegistrationRequest { get; set; }

    /// <summary>
    /// Type 1: إلغاء قرار إداري (Management Decision Cancellation) - Optional
    /// </summary>
    public virtual AdditionalInfoManagementDecision? ManagementDecision { get; set; }

    /// <summary>
    /// Type 2: حقوق خدمة/تقاعدية (Service/Retirement Rights) - Optional
    /// </summary>
    public virtual AdditionalInfoServiceRights? ServiceRights { get; set; }

    /// <summary>
    /// Type 3: نزاع علامة تجارية (Trademark Dispute) - Optional
    /// </summary>
    public virtual AdditionalInfoTrademark? Trademark { get; set; }

    #endregion
}
