using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Repository interface for AdditionalInfo entity.
/// Defines data access operations for additional information related to case registration requests.
/// </summary>
public interface IAdditionalInfoRepository : IRepository<AdditionalInfo>
{
    /// <summary>
    /// Gets additional information by case registration request ID (read-only).
    /// Returns null if not found or if soft deleted.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AdditionalInfo entity or null if not found</returns>
    Task<AdditionalInfo?> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets additional information by case registration request ID with tracking enabled for updates.
    /// Used for update operations to ensure nested navigation property changes are tracked by EF Core.
    /// Returns null if not found or if soft deleted.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tracked AdditionalInfo entity or null if not found</returns>
    Task<AdditionalInfo?> GetByRequestIdAsyncForUpdateAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if additional information exists for a request.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if additional info exists and is not deleted; otherwise false</returns>
    Task<bool> ExistsByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);
}
