using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

public interface IClaimRepository : IRepository<Claim>
{
    Task<IEnumerable<Claim>> GetByRequestIdAsync(int requestId, CancellationToken ct = default);
    Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default);
}
