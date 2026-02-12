namespace BOG.DbModel.Entities.Lookups;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Lookup entity for طريقة العلم بالقرار (Method of Notification of Decision).
/// Used in AdditionalInfoManagementDecision to specify how the decision was notified.
/// </summary>
public class NotificationMethod : BaseEntity
{
    /// <summary>
    /// Method name in English.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Method name in Arabic.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Description of the notification method.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this notification method is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
