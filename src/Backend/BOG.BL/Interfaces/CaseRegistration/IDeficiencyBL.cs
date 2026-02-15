using BOG.DTO.CaseRegistration;
using BOG.VM.CaseRegistration;

namespace BOG.BL.Interfaces.CaseRegistration;

/// <summary>
/// Business logic interface for Deficiencies (نواقص الدعوى).
/// Handles deficiency retrieval and batch updates.
/// </summary>
public interface IDeficiencyBL
{
    /// <summary>
    /// Gets all deficiencies for a request.
    /// </summary>
    Task<IEnumerable<DeficiencyVM>> GetDeficienciesAsync(int requestId, CancellationToken ct = default);

    /// <summary>
    /// Replaces all deficiencies for a request with the provided list.
    /// Soft deletes existing deficiencies and creates new ones.
    /// Validates request exists and is in editable state (Draft or Deficiencies).
    /// </summary>
    Task<IEnumerable<DeficiencyVM>> UpdateDeficienciesAsync(int requestId, DeficienciesBatchUpdateDTO dto, CancellationToken ct = default);
}
