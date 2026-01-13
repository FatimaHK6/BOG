namespace BOG.VM.User;

/// <summary>
/// User ViewModel for presentation layer/API response.
/// Follows Single Responsibility Principle - handles only presentation data structure for users.
/// May include computed properties for display purposes.
/// </summary>
public class UserVM
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
    /// User's full name (computed property).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

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
    /// User's status display (Active/Inactive).
    /// Computed property for UI display.
    /// </summary>
    public string StatusDisplay => IsActive ? "Active" : "Inactive";
}
