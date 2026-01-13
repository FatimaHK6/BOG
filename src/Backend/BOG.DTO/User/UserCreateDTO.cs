namespace BOG.DTO.User;

/// <summary>
/// DTO for creating a new user.
/// Follows Single Responsibility Principle - handles only data transfer for user creation.
/// Follows Interface Segregation Principle - contains only properties needed for creation.
/// </summary>
public class UserCreateDTO
{
    /// <summary>
    /// User's email address (required, must be unique).
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's first name (required).
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// User's last name (required).
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// User's phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; set; }
}
