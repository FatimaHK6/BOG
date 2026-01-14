using System.ComponentModel.DataAnnotations;

namespace BOG.DTO.Role;

/// <summary>
/// DTO for creating a new role.
/// </summary>
public class RoleCreateDTO
{
    /// <summary>
    /// Role name in English (required).
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Role name in Arabic (required).
    /// </summary>
    [Required(ErrorMessage = "Arabic name is required")]
    [MaxLength(100)]
    public string NameAr { get; set; } = null!;

    /// <summary>
    /// Role description (optional).
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}
