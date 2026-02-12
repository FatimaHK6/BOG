using BOG.DTO.Defendant;
using BOG.VM.Defendant;

namespace BOG.BL.Interfaces;

/// <summary>
/// Defendant business logic interface.
/// </summary>
public interface IDefendantBL
{
    /// <summary>
    /// Gets all defendants for a case request.
    /// </summary>
    Task<IEnumerable<DefendantListVM>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a defendant by ID with full details.
    /// </summary>
    Task<DefendantVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new defendant and associates with a case request.
    /// </summary>
    Task<DefendantVM> CreateAsync(int requestId, DefendantCreateDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a defendant.
    /// </summary>
    Task<DefendantVM?> UpdateAsync(int id, DefendantCreateDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a defendant (soft delete).
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
