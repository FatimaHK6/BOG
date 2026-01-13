namespace BOG.DTO.User;

/// <summary>
/// DTO for reading/retrieving user data.
/// Follows Single Responsibility Principle - handles only data transfer for user retrieval.
/// </summary>
public class UserDTO
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User's email address.
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
    /// User's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates if the user account is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time when the user was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Date and time when the user was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
