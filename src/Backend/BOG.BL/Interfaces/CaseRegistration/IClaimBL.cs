using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Interfaces.CaseRegistration;

public interface IClaimBL
{
    Task<IEnumerable<ClaimVM>> GetClaimsAsync(int requestId, CancellationToken ct = default);
    Task<IEnumerable<ClaimVM>> UpdateClaimsAsync(int requestId, ClaimsBatchUpdateDTO dto, CancellationToken ct = default);
}
