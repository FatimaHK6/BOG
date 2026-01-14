using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Role;

/// <summary>
/// DTO for updating a role.
/// </summary>
public class RoleUpdateDTO
{
    /// <summary>
    /// Role name in English.
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// Role name in Arabic.
    /// </summary>
    [MaxLength(100)]
    public string? NameAr { get; set; }

    /// <summary>
    /// Role description.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the role is active.
    /// </summary>
    public bool? IsActive { get; set; }
}
