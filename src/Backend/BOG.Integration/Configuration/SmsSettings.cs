namespace BOG.Integration.Configuration;

/// <summary>
/// Configuration settings for SMS service.
/// </summary>
public class SmsSettings
{
    /// <summary>
    /// Whether to use mock SMS service instead of real provider.
    /// </summary>
    public bool UseMock { get; set; } = true;

    /// <summary>
    /// Sender name for SMS messages.
    /// </summary>
    public string SenderName { get; set; } = "MOJ-BOG";

    /// <summary>
    /// API endpoint for SMS provider.
    /// </summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// API key for SMS provider authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Account username for SMS provider.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Account password for SMS provider.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
