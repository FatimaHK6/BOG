using BOG.DTO.Plaintiff;
using BOG.VM.Common;
using BOG.VM.Plaintiff;

namespace BOG.BL.Interfaces;

/// <summary>
/// Plaintiff business logic interface.
/// Handles plaintiff management operations.
/// </summary>
public interface IPlaintiffBL
{
    /// <summary>
    /// Gets all plaintiffs for a case request.
    /// </summary>
    Task<IEnumerable<PlaintiffListVM>> GetByRequestIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plaintiff by ID with full details.
    /// </summary>
    Task<PlaintiffVM?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new plaintiff and associates with a case request.
    /// Includes Absher integration for Individual type.
    /// </summary>
    Task<PlaintiffVM> CreateAsync(int requestId, PlaintiffCreateDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a plaintiff.
    /// </summary>
    Task<PlaintiffVM> UpdateAsync(int id, PlaintiffUpdateDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a plaintiff (soft delete).
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a plaintiff as the applicant (مقدم الطلب).
    /// Only Individual (type 1, 2) can be applicant (BC07).
    /// </summary>
    Task<PlaintiffVM> SetAsApplicantAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the selected address for a plaintiff.
    /// </summary>
    Task<AddressVM?> GetSelectedAddressAsync(int plaintiffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the selected address for notifications (العنوان المختار).
    /// Required when plaintiff city differs from court city (BC03).
    /// </summary>
    Task<AddressVM> SetSelectedAddressAsync(int plaintiffId, SelectedAddressDTO dto, CancellationToken cancellationToken = default);
}
