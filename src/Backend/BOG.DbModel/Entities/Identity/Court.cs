namespace BOG.DbModel.Entities.Identity;

/// <summary>
/// Court entity representing an administrative court.
/// المحكمة الإدارية
/// </summary>
public class Court : BaseEntity
{
    /// <summary>
    /// Court name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Court name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Region ID where the court is located.
    /// </summary>
    public int RegionId { get; set; }

    /// <summary>
    /// City ID where the court is located.
    /// </summary>
    public int CityId { get; set; }

    /// <summary>
    /// Indicates if the court is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
