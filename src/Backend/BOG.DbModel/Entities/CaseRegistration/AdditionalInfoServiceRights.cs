namespace BOG.DbModel.Entities.CaseRegistration;

using System.ComponentModel.DataAnnotations;
using BOG.DbModel.Entities.Lookups;

/// <summary>
/// Type 2: حقوق خدمة/تقاعدية (Service/Retirement Rights)
/// Stores complaint and service rights information for service/retirement rights cases.
/// One-to-one relationship with AdditionalInfo.
/// </summary>
public class AdditionalInfoServiceRights : BaseEntity
{
    /// <summary>
    /// Foreign key to AdditionalInfo (parent).
    /// </summary>
    public int AdditionalInfoId { get; set; }

    /// <summary>
    /// يوجد تظلم - Has Complaint (Yes/No flag).
    /// </summary>
    public bool? HasComplaint { get; set; }

    /// <summary>
    /// رقم التظلم - Complaint Number.
    /// </summary>
    [MaxLength(50)]
    public string? ComplaintNumber { get; set; }

    /// <summary>
    /// تاريخ التظلم - Complaint Date.
    /// </summary>
    public DateTime? ComplaintDate { get; set; }

    /// <summary>
    /// Foreign key to GovernmentEntity lookup table.
    /// الجهة المتظلم لها - Authority Complained To.
    /// </summary>
    public int? ComplaintAuthorityId { get; set; }

    /// <summary>
    /// تاريخ البت في التظلم - Date of Decision on Complaint.
    /// </summary>
    public DateTime? ComplaintDecisionDate { get; set; }

    /// <summary>
    /// نتيجة النظام - System Result (free text, max 500 characters).
    /// </summary>
    [MaxLength(500)]
    public string? SystemResult { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to parent AdditionalInfo.
    /// </summary>
    public virtual AdditionalInfo AdditionalInfo { get; set; } = null!;

    /// <summary>
    /// Navigation property to GovernmentEntity lookup (optional).
    /// </summary>
    public virtual GovernmentEntity? ComplaintAuthority { get; set; }

    #endregion
}
