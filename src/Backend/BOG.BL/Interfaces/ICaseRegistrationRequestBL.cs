using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistrationRequest;

namespace BOG.BL.Interfaces;

/// <summary>
/// Business logic interface for CaseRegistrationRequest.
/// </summary>
public interface ICaseRegistrationRequestBL
{
    /// <summary>
    /// Gets all requests.
    /// </summary>
    Task<IEnumerable<CaseRegistrationRequestListVM>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all requests for a specific user.
    /// </summary>
    Task<IEnumerable<CaseRegistrationRequestListVM>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a request by ID with full details.
    /// </summary>
    Task<CaseRegistrationRequestVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new draft request.
    /// </summary>
    Task<CaseRegistrationRequestVM> CreateDraftAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a request (soft delete).
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a case registration request.
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <param name="dto">Update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated request or null if not found</returns>
    /// <exception cref="InvalidOperationException">If request cannot be updated (wrong status)</exception>
    Task<CaseRegistrationRequestVM?> UpdateAsync(
        int id,
        CaseRegistrationUpdateDTO dto,
        CancellationToken cancellationToken = default);
}
