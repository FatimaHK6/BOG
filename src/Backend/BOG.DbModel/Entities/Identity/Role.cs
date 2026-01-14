namespace BOG.DbModel.Entities.Identity;

/// <summary>
/// Role entity representing a system role.
/// أدوار النظام
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Role name in English (Clerk, Reviewer, Judge, etc.)
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Role name in Arabic (كاتب، مدقق، قاضي، etc.)
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Role description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if the role is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
