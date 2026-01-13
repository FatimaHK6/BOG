namespace BOG.DAL.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions.
/// Follows Single Responsibility Principle and Dependency Inversion Principle.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Begins a database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <returns>A repository instance for the entity type</returns>
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
}
