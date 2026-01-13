namespace BOG.DbModel.Entities;

/// <summary>
/// Base entity class for all database entities.
/// Follows Single Responsibility Principle - contains only common entity properties.
/// Follows Open-Closed Principle - extendable for specific entity requirements.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Date and time when the entity was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the entity was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag. Entities with IsDeleted = true are considered deleted
    /// but remain in the database for audit purposes.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
