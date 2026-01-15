using BOG.DAL.Interfaces;
using BOG.DbModel;
using BOG.DbModel.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace BOG.DAL.Repositories;

/// <summary>
/// Address repository implementation.
/// Follows SOLID principles - handles only address data access.
/// </summary>
public class AddressRepository : Repository<Address>, IAddressRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public AddressRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _applicationDbContext = dbContext;
    }

    public async Task<IEnumerable<Address>> GetByCityIdAsync(int cityId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.CityId == cityId && !a.IsDeleted && a.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Address>> GetByPostalCodeAsync(string postalCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return Enumerable.Empty<Address>();

        return await _dbSet
            .AsNoTracking()
            .Where(a => a.PostalCode == postalCode && !a.IsDeleted && a.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address> CreateFromAbsherDataAsync(
        string buildingNumber,
        string streetName,
        string district,
        string city,
        string postalCode,
        string additionalNumber,
        string addressType,
        CancellationToken cancellationToken = default)
    {
        var address = new Address
        {
            BuildingNumber = buildingNumber,
            StreetName = streetName,
            District = district,
            City = city,
            PostalCode = postalCode,
            AdditionalNumber = additionalNumber,
            AddressType = addressType,
            FullAddress = $"{buildingNumber} {streetName}, {district}, {city} {postalCode}",
            IsActive = true,
            IsPrimary = false,
            CreatedDate = DateTime.UtcNow
        };

        await _dbSet.AddAsync(address, cancellationToken);
        return address;
    }
}
