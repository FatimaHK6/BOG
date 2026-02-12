namespace BOG.Integration.Configuration;

/// <summary>
/// Configuration settings for case management system integration.
/// </summary>
public class CaseManagementSettings
{
    /// <summary>
    /// Whether to use mock case management service instead of real system.
    /// </summary>
    public bool UseMock { get; set; } = true;

    /// <summary>
    /// API endpoint for case management system.
    /// </summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// API key for case management system authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Username for case management system.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for case management system.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Mock case number prefix for testing.
    /// </summary>
    public string MockCaseNumberPrefix { get; set; } = "TEST-CASE-";

    /// <summary>
    /// Mock registration number prefix for testing.
    /// </summary>
    public string MockRegistrationNumberPrefix { get; set; } = "TEST-REG-";

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
