namespace BOG.DbModel.Entities.CaseRegistration;

using System.ComponentModel.DataAnnotations;
using BOG.DbModel.Entities.Lookups;

/// <summary>
/// Type 1: إلغاء قرار إداري (Management Decision Cancellation)
/// Stores decision-related information for administrative decision cancellation cases.
/// One-to-one relationship with AdditionalInfo.
/// </summary>
public class AdditionalInfoManagementDecision : BaseEntity
{
    /// <summary>
    /// Foreign key to AdditionalInfo (parent).
    /// </summary>
    public int AdditionalInfoId { get; set; }

    /// <summary>
    /// رقم القرار - Decision Number.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DecisionNumber { get; set; } = null!;

    /// <summary>
    /// تاريخ القرار - Decision Date.
    /// </summary>
    [Required]
    public DateTime DecisionDate { get; set; }

    /// <summary>
    /// تاريخ العلم بالقرار - Date of Notification of Decision.
    /// </summary>
    [Required]
    public DateTime NotificationDate { get; set; }

    /// <summary>
    /// Foreign key to NotificationMethod lookup table.
    /// طريقة العلم بالقرار - Method of Notification of Decision.
    /// </summary>
    public int? NotificationMethodId { get; set; }

    /// <summary>
    /// Foreign key to GovernmentEntity lookup table.
    /// جهة إصدار القرار - Decision Issuing Authority.
    /// </summary>
    public int? IssuingAuthorityId { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to parent AdditionalInfo.
    /// </summary>
    public virtual AdditionalInfo AdditionalInfo { get; set; } = null!;

    /// <summary>
    /// Navigation property to NotificationMethod lookup.
    /// </summary>
    public virtual NotificationMethod? NotificationMethod { get; set; }

    /// <summary>
    /// Navigation property to GovernmentEntity lookup.
    /// </summary>
    public virtual GovernmentEntity? IssuingAuthority { get; set; }

    #endregion
}
