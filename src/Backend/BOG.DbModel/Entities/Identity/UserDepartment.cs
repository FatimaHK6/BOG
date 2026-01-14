namespace BOG.DbModel.Entities.Identity;

/// <summary>
/// Junction table for User-Department many-to-many relationship.
/// جدول ربط المستخدم بالدائرة
/// </summary>
public class UserDepartment : BaseEntity
{
    /// <summary>
    /// Foreign key to User.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Foreign key to Department.
    /// </summary>
    public int DepartmentId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
}
