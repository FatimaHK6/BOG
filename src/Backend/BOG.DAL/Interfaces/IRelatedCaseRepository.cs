using BOG.DbModel.Entities.CaseRegistration;

namespace BOG.DAL.Interfaces;

public interface IRelatedCaseRepository : IRepository<RelatedCase>
{
    Task<IEnumerable<RelatedCase>> GetByRequestIdAsync(int requestId, CancellationToken ct = default);
    Task DeleteByRequestIdAsync(int requestId, CancellationToken ct = default);
}
