namespace BOG.DTO.User;

/// <summary>
/// DTO for updating a user.
/// Follows Single Responsibility Principle - handles only data transfer for user updates.
/// Follows Interface Segregation Principle - contains only properties that can be updated.
/// </summary>
public class UserUpdateDTO
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }
}
