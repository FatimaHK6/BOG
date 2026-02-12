namespace BOG.DbModel.Entities.CaseRegistration;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Type 3: نزاع علامة تجارية (Trademark Dispute)
/// Stores trademark request information for trademark dispute cases.
/// One-to-one relationship with AdditionalInfo.
/// </summary>
public class AdditionalInfoTrademark : BaseEntity
{
    /// <summary>
    /// Foreign key to AdditionalInfo (parent).
    /// </summary>
    public int AdditionalInfoId { get; set; }

    /// <summary>
    /// رقم الطلب - Request Number (Trademark application/request number).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RequestNumber { get; set; } = null!;

    /// <summary>
    /// تاريخه - Request Date (Date of the trademark request).
    /// </summary>
    [Required]
    public DateTime RequestDate { get; set; }

    #region Navigation Properties

    /// <summary>
    /// Navigation property to parent AdditionalInfo.
    /// </summary>
    public virtual AdditionalInfo AdditionalInfo { get; set; } = null!;

    #endregion
}
