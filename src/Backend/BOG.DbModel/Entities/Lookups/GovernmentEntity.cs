namespace BOG.DbModel.Entities.Lookups;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Lookup entity for government entities (جهات حكومية).
/// Used in AdditionalInfo types for specifying:
/// - جهة إصدار القرار (Decision Issuing Authority) in Type 1
/// - الجهة المتظلم لها (Authority Complained To) in Type 2
/// </summary>
public class GovernmentEntity : BaseEntity
{
    /// <summary>
    /// Entity name in English.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Entity name in Arabic.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Entity code (e.g., Ministry code).
    /// </summary>
    [MaxLength(50)]
    public string? Code { get; set; }

    /// <summary>
    /// Description of the government entity.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this entity is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
