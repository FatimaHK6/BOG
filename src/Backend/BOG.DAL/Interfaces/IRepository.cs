namespace BOG.DAL.Interfaces;

/// <summary>
/// Generic repository interface following Dependency Inversion Principle.
/// Defines contract for data access operations.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The search condition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching entities</returns>
    Task<IEnumerable<TEntity>> FindAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists that matches the predicate.
    /// </summary>
    /// <param name="predicate">The search condition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added entity</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
