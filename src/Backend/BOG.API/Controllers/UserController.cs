using BOG.BL.Interfaces;
using BOG.DTO.User;
using Microsoft.AspNetCore.Mvc;

namespace BOG.API.Controllers;

/// <summary>
/// User management API controller.
/// Follows Single Responsibility Principle - handles only HTTP requests/responses for users.
/// Follows Dependency Inversion Principle - depends on IUserBL abstraction.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserBL _userBL;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserBL userBL, ILogger<UsersController> logger)
    {
        _userBL = userBL ?? throw new ArgumentNullException(nameof(userBL));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="createDto">The user data to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreateUser([FromBody] UserCreateDTO createDto, CancellationToken cancellationToken)
    {
        try
        {
            var userVM = await _userBL.CreateUserAsync(createDto, cancellationToken);
            _logger.LogInformation("User created with ID: {UserId}", userVM.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = userVM.Id }, userVM);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create user: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the user." });
        }
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserById([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var userVM = await _userBL.GetUserByIdAsync(id, cancellationToken);
            if (userVM == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            return Ok(userVM);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user with ID: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the user." });
        }
    }

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user details</returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserByEmail([FromRoute] string email, CancellationToken cancellationToken)
    {
        try
        {
            var userVM = await _userBL.GetUserByEmailAsync(email, cancellationToken);
            if (userVM == null)
                return NotFound(new { message = $"User with email '{email}' not found." });

            return Ok(userVM);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user with email: {Email}", email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the user." });
        }
    }

    /// <summary>
    /// Gets all active users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active users</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllActiveUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userBL.GetAllActiveUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving active users");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving users." });
        }
    }

    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="updateDto">The updated user data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateUser([FromRoute] int id, [FromBody] UserUpdateDTO updateDto, CancellationToken cancellationToken)
    {
        try
        {
            var userVM = await _userBL.UpdateUserAsync(id, updateDto, cancellationToken);
            _logger.LogInformation("User with ID {UserId} updated", id);
            return Ok(userVM);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update user: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating user with ID: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the user." });
        }
    }

    /// <summary>
    /// Deletes a user (soft delete).
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            await _userBL.DeleteUserAsync(id, cancellationToken);
            _logger.LogInformation("User with ID {UserId} deleted", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to delete user: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting user with ID: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the user." });
        }
    }

    /// <summary>
    /// Sets the active status of a user.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="isActive">Whether the user should be active</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user details</returns>
    [HttpPatch("{id}/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetUserActiveStatus([FromRoute] int id, [FromQuery] bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            var userVM = await _userBL.SetUserActiveStatusAsync(id, isActive, cancellationToken);
            _logger.LogInformation("User with ID {UserId} active status set to {IsActive}", id, isActive);
            return Ok(userVM);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update user active status: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating user active status for ID: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the user's active status." });
        }
    }
}
