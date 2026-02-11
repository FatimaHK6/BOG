using BOG.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BOG.DAL.Repositories;

/// <summary>
/// Unit of Work pattern implementation for managing database transactions.
/// Follows Single Responsibility Principle - manages only transactions and repository coordination.
/// Follows Dependency Inversion Principle - depends on abstraction (DbContext).
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _transaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _transaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void ClearChangeTracker()
    {
        _dbContext.ChangeTracker.Clear();
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (!_repositories.TryGetValue(type, out var repository))
        {
            var repositoryType = typeof(Repository<>).MakeGenericType(type);
            repository = Activator.CreateInstance(repositoryType, _dbContext)!;
            _repositories.Add(type, repository);
        }

        return (IRepository<TEntity>)repository;
    }

    public async ValueTask DisposeAsync()
    {
        _transaction?.Dispose();
        await _dbContext.DisposeAsync();
    }
}
