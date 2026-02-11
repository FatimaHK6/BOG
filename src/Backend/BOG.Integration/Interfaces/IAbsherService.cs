using BOG.Integration.DTOs.Absher;

namespace BOG.Integration.Interfaces;

/// <summary>
/// Interface for Absher (National Information Center) integration.
/// Used for identity verification and fetching personal data.
/// </summary>
public interface IAbsherService
{
    /// <summary>
    /// Verifies an identity number with Absher.
    /// </summary>
    /// <param name="identityNumber">The identity number to verify</param>
    /// <param name="identityType">Identity type: 1=National ID, 2=Resident ID, 3=Passport</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification result</returns>
    Task<AbsherVerificationResult> VerifyIdentityAsync(string identityNumber, int identityType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets personal data from Absher for a verified identity.
    /// </summary>
    /// <param name="identityNumber">The identity number</param>
    /// <param name="identityType">Identity type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Personal data if found, null otherwise</returns>
    Task<AbsherPersonData?> GetPersonDataAsync(string identityNumber, int identityType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets national address data from Absher.
    /// </summary>
    /// <param name="identityNumber">The identity number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Address data if found, null otherwise</returns>
    Task<AbsherAddressData?> GetNationalAddressAsync(string identityNumber, CancellationToken cancellationToken = default);
}
