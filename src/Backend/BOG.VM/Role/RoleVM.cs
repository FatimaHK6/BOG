namespace BOG.VM.Role;

/// <summary>
/// ViewModel for Role entity.
/// </summary>
public class RoleVM
{
    /// <summary>
    /// Role ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Role name in English.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Role name in Arabic.
    /// </summary>
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Role description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the role is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date when the role was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
