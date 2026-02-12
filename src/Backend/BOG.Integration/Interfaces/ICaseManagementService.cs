using BOG.Integration.DTOs.CaseManagement;

namespace BOG.Integration.Interfaces;

/// <summary>
/// Service for case management system integration.
/// </summary>
public interface ICaseManagementService
{
    /// <summary>
    /// Register a new case in the case management system.
    /// </summary>
    /// <param name="registrationData">Case registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration result with case number</returns>
    Task<CaseRegistrationResult> RegisterCaseAsync(CaseRegistrationData registrationData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the status of a registered case.
    /// </summary>
    /// <param name="caseNumber">Case number to query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Case status result</returns>
    Task<CaseStatusResult> GetCaseStatusAsync(string caseNumber, CancellationToken cancellationToken = default);
}
