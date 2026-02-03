using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Defendant-specific repository interface.
/// </summary>
public interface IDefendantRepository : IRepository<Defendant>
{
    /// <summary>
    /// Gets all defendants for a specific case request.
    /// </summary>
    Task<IEnumerable<Defendant>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a defendant with all related details.
    /// </summary>
    Task<Defendant?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a defendant with the same identity number and type already exists for the request.
    /// </summary>
    Task<bool> ExistsDuplicateAsync(int requestId, string identityNumber, int defendantTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a defendant with the same identity number and type already exists for the request, excluding a specific defendant.
    /// </summary>
    Task<bool> ExistsDuplicateExcludingAsync(int requestId, string identityNumber, int defendantTypeId, int excludeDefendantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the request ID for a defendant.
    /// </summary>
    Task<int?> GetRequestIdAsync(int defendantId, CancellationToken cancellationToken = default);
}
