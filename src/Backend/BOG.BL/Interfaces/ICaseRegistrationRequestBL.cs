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
    /// Updates a request (saves as draft).
    /// </summary>
    Task<CaseRegistrationRequestVM?> UpdateAsync(int id, bool saveAsDraft, CancellationToken cancellationToken = default);
}
