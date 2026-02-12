using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Interfaces.CaseRegistration;

public interface IRelatedCaseBL
{
    Task<IEnumerable<RelatedCaseVM>> GetRelatedCasesAsync(int requestId, CancellationToken ct = default);
    Task<IEnumerable<RelatedCaseVM>> UpdateRelatedCasesAsync(int requestId, RelatedCasesBatchUpdateDTO dto, CancellationToken ct = default);
}
