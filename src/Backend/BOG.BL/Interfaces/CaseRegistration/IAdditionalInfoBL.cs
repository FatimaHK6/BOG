using BOG.VM.AdditionalInfo;

namespace BOG.BL.Interfaces.CaseRegistration;

/// <summary>
/// Business logic service for additional information related to case registration requests.
/// Handles supplementary case details, notes, and metadata.
/// </summary>
public interface IAdditionalInfoBL
{
    /// <summary>
    /// Adds or updates additional information for a case registration request.
    /// If additional info already exists for the request, updates it.
    /// Otherwise, creates new record.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="additionalInfoData">Additional information data (notes, references, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added or updated additional info view model</returns>
    Task<AdditionalInfoVM> AddOrUpdateAsync(int requestId, object additionalInfoData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets additional information for a case registration request.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Additional info view model or null if not found</returns>
    Task<AdditionalInfoVM?> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets additional information by ID.
    /// </summary>
    /// <param name="additionalInfoId">Additional info record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Additional info view model or null if not found</returns>
    Task<AdditionalInfoVM?> GetByIdAsync(int additionalInfoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes additional information for a case registration request.
    /// Soft delete via IsDeleted flag.
    /// </summary>
    /// <param name="requestId">Case registration request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing async operation</returns>
    Task DeleteByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes additional information by ID.
    /// </summary>
    /// <param name="additionalInfoId">Additional info record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing async operation</returns>
    Task DeleteAsync(int additionalInfoId, CancellationToken cancellationToken = default);
}
