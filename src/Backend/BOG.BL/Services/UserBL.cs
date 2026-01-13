using BOG.BL.Interfaces;
using BOG.DAL.Interfaces;
using BOG.DbModel.Entities;
using BOG.DTO.User;
using BOG.VM.User;

namespace BOG.BL.Services;

/// <summary>
/// User business logic service.
/// Follows Single Responsibility Principle - handles only user business logic.
/// Follows Dependency Inversion Principle - depends on IUserRepository abstraction.
/// Follows Open-Closed Principle - can be extended for additional business rules.
/// </summary>
public class UserBL : IUserBL
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserBL(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<UserVM> CreateUserAsync(UserCreateDTO createDto, CancellationToken cancellationToken = default)
    {
        if (createDto == null)
            throw new ArgumentNullException(nameof(createDto));

        // Business rule: Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(createDto.Email, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException($"A user with email '{createDto.Email}' already exists.");

        var user = new User
        {
            Email = createDto.Email,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            PhoneNumber = createDto.PhoneNumber,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return MapToViewModel(user);
    }

    public async Task<UserVM?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.IsDeleted)
            return null;

        return MapToViewModel(user);
    }

    public async Task<UserVM?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null)
            return null;

        return MapToViewModel(user);
    }

    public async Task<IEnumerable<UserVM>> GetAllActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllActiveAsync(cancellationToken);
        return users.Select(MapToViewModel).ToList();
    }

    public async Task<UserVM> UpdateUserAsync(int userId, UserUpdateDTO updateDto, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new InvalidOperationException($"User with ID {userId} not found.");

        // Update only provided properties
        if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
            user.FirstName = updateDto.FirstName;

        if (!string.IsNullOrWhiteSpace(updateDto.LastName))
            user.LastName = updateDto.LastName;

        if (updateDto.PhoneNumber != null)
            user.PhoneNumber = updateDto.PhoneNumber;

        user.ModifiedDate = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return MapToViewModel(user);
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new InvalidOperationException($"User with ID {userId} not found.");

        // Soft delete
        user.IsDeleted = true;
        user.ModifiedDate = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserVM> SetUserActiveStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than 0.", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new InvalidOperationException($"User with ID {userId} not found.");

        user.IsActive = isActive;
        user.ModifiedDate = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return MapToViewModel(user);
    }

    /// <summary>
    /// Maps a User entity to UserVM (ViewModel).
    /// Helper method following single responsibility principle.
    /// </summary>
    private static UserVM MapToViewModel(User user)
    {
        return new UserVM
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate
        };
    }
}
