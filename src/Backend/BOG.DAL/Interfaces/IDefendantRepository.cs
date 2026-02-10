using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Repository interface for Defendant entity.
/// Extends generic IRepository with defendant-specific queries.
/// Follows Repository Pattern and Dependency Inversion Principle.
/// </summary>
public interface IDefendantRepository : IRepository<Defendant>
{
    /// <summary>
    /// Gets all defendants for a specific case registration request.
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerable of defendants in the request, excluding deleted ones</returns>
    Task<IEnumerable<Defendant>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a defendant by identity number within a request (checks for duplicates - ERR013).
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="identityNumber">The defendant's identity number</param>
    /// <param name="defendantTypeId">The defendant type ID (must match for duplicate check)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The defendant if found, null otherwise</returns>
    Task<Defendant?> GetByIdentityAsync(int requestId, string identityNumber, int defendantTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a defendant with the same identity already exists in the request (ERR013 - Duplicate Check).
    /// </summary>
    /// <param name="requestId">The case registration request ID</param>
    /// <param name="identityNumber">The defendant's identity number</param>
    /// <param name="defendantTypeId">The defendant type ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if defendant exists, false otherwise</returns>
    Task<bool> ExistsByIdentityAsync(int requestId, string identityNumber, int defendantTypeId, CancellationToken cancellationToken = default);
}
