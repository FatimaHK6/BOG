namespace BOG.DbModel.Entities;

/// <summary>
/// User entity representing a system user.
/// Inherits from BaseEntity to gain common properties (Id, CreatedDate, ModifiedDate, IsDeleted).
/// Follows Single Responsibility Principle - represents only user data.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// User's email address (unique and required).
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// User's phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates if the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
