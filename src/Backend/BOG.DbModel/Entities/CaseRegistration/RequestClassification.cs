using BOG.DbModel.Entities.Lookups;
using System.ComponentModel.DataAnnotations.Schema;

namespace BOG.DbModel.Entities.CaseRegistration;

/// <summary>
/// Entity for case classification (تصنيف الدعوى) - one-to-many with CaseRegistrationRequest.
/// </summary>
public class RequestClassification : BaseEntity
{
    /// <summary>
    /// Foreign key to CaseRegistrationRequest.
    /// </summary>
    public int CaseRegistrationRequestId { get; set; }

    /// <summary>
    /// Foreign key to Classification lookup.
    /// </summary>
    public int ClassificationId { get; set; }

    /// <summary>
    /// Classification text (نص التصنيف) - DEPRECATED: No longer used.
    /// </summary>
    [NotMapped]
    public string? ClassificationText { get; set; }

    /// <summary>
    /// Display order for sorting classifications.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Navigation property for the request.
    /// </summary>
    public virtual CaseRegistrationRequest Request { get; set; } = null!;

    /// <summary>
    /// Navigation property for the classification lookup.
    /// </summary>
    public virtual Classification Classification { get; set; } = null!;
}
