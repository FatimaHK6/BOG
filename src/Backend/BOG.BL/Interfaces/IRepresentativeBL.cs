using BOG.DTO.Representative;
using BOG.VM.Representative;

namespace BOG.BL.Interfaces;

/// <summary>
/// Representative business logic interface.
/// Handles representative management operations.
/// </summary>
public interface IRepresentativeBL
{
    /// <summary>
    /// Gets all representatives for a plaintiff.
    /// </summary>
    Task<IEnumerable<RepresentativeVM>> GetByPlaintiffIdAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a representative by ID with full details.
    /// </summary>
    Task<RepresentativeVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new representative for a plaintiff.
    /// Validates representative type allowed for plaintiff type.
    /// Includes Absher integration.
    /// </summary>
    Task<RepresentativeVM> CreateAsync(int plaintiffId, RepresentativeCreateDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a representative.
    /// </summary>
    Task<RepresentativeVM> UpdateAsync(int id, RepresentativeUpdateDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a representative (soft delete).
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets allowed representative types for a plaintiff type.
    /// </summary>
    IEnumerable<int> GetAllowedRepresentativeTypes(int plaintiffTypeId);
}
