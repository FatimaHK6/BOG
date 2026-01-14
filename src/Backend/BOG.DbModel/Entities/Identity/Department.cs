namespace BOG.DbModel.Entities.Identity;

/// <summary>
/// Department entity representing a department within a court.
/// الدائرة داخل المحكمة
/// </summary>
public class Department : BaseEntity
{
    /// <summary>
    /// Foreign key to Court.
    /// </summary>
    public int CourtId { get; set; }

    /// <summary>
    /// Department name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Department name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Indicates if the department is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Court Court { get; set; } = null!;
    public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
}
