using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Representative-specific repository interface.
/// Follows Interface Segregation Principle - defines only representative-related operations.
/// </summary>
public interface IRepresentativeRepository : IRepository<Representative>
{
    /// <summary>
    /// Gets all representatives for a specific plaintiff.
    /// </summary>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of representatives</returns>
    Task<IEnumerable<Representative>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a representative by identity number for a specific plaintiff.
    /// </summary>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="identityNumber">The identity number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The representative if found, null otherwise</returns>
    Task<Representative?> GetByIdentityAsync(int plaintiffId, string identityNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a representative with the given identity exists for a plaintiff.
    /// Used for duplicate checking (ERR008).
    /// </summary>
    /// <param name="plaintiffId">The plaintiff ID</param>
    /// <param name="identityNumber">The identity number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if representative exists, false otherwise</returns>
    Task<bool> ExistsByIdentityAsync(int plaintiffId, string identityNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a representative with all related details.
    /// </summary>
    /// <param name="id">The representative ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The representative with details if found, null otherwise</returns>
    Task<Representative?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
