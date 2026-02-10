using BOG.VM.Defendant;

namespace BOG.BL.Interfaces.CaseRegistration;

/// <summary>
/// Business logic service for defendant management in case registration.
/// Handles defendant creation, validation, and retrieval.
/// </summary>
public interface IDefendantBL
{
    /// <summary>
    /// Creates a new defendant for a case registration request.
    /// Validates business rules including ERR013 (duplicate identity check).
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="defendantData">Defendant data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created defendant view model</returns>
    Task<DefendantVM> CreateDefendantAsync(int requestId, object defendantData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a defendant by ID.
    /// </summary>
    /// <param name="defendantId">Defendant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Defendant view model or null if not found</returns>
    Task<DefendantVM?> GetDefendantByIdAsync(int defendantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all defendants for a specific case registration request.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of defendant view models</returns>
    Task<IEnumerable<DefendantListVM>> GetDefendantsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a defendant.
    /// </summary>
    /// <param name="defendantId">Defendant ID</param>
    /// <param name="defendantData">Updated defendant data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated defendant view model</returns>
    Task<DefendantVM> UpdateDefendantAsync(int defendantId, object defendantData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a defendant.
    /// </summary>
    /// <param name="defendantId">Defendant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteDefendantAsync(int defendantId, CancellationToken cancellationToken = default);
}
