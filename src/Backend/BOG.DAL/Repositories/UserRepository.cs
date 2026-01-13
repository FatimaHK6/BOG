using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// User repository implementation.
/// Follows Single Responsibility Principle - handles only user data access.
/// Follows Open-Closed Principle - extends Repository<User> without modifying base class.
/// Follows Liskov Substitution Principle - can be substituted for IUserRepository.
/// Follows Dependency Inversion Principle - depends on DbContext abstraction.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }
}
