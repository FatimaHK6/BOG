using BOG.DbModel.Entities.Common;

namespace BOG.DAL.Interfaces;

/// <summary>
/// Address-specific repository interface.
/// Follows Interface Segregation Principle - defines only address-related operations.
/// </summary>
public interface IAddressRepository : IRepository<Address>
{
    /// <summary>
    /// Gets an address by city ID.
    /// </summary>
    /// <param name="cityId">The city ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of addresses in the city</returns>
    Task<IEnumerable<Address>> GetByCityIdAsync(int cityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an address by postal code.
    /// </summary>
    /// <param name="postalCode">The postal code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of addresses with the postal code</returns>
    Task<IEnumerable<Address>> GetByPostalCodeAsync(string postalCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an address from Absher data.
    /// </summary>
    /// <param name="buildingNumber">Building number</param>
    /// <param name="streetName">Street name</param>
    /// <param name="district">District</param>
    /// <param name="city">City name</param>
    /// <param name="postalCode">Postal code</param>
    /// <param name="additionalNumber">Additional number</param>
    /// <param name="addressType">Address type (Residence/Work)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created address</returns>
    Task<Address> CreateFromAbsherDataAsync(
        string buildingNumber,
        string streetName,
        string district,
        string city,
        string postalCode,
        string additionalNumber,
        string addressType,
        CancellationToken cancellationToken = default);
}
