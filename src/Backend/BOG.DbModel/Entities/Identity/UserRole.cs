namespace BOG.DbModel.Entities.Identity;

/// <summary>
/// Junction table for User-Role many-to-many relationship.
/// جدول ربط المستخدم بالدور
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// Foreign key to User.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Foreign key to Role.
    /// </summary>
    public int RoleId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
