namespace BOG.Integration.Configuration;

/// <summary>
/// Configuration settings for email service.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Whether to use mock email service instead of real provider.
    /// </summary>
    public bool UseMock { get; set; } = true;

    /// <summary>
    /// From email address.
    /// </summary>
    public string FromEmail { get; set; } = "noreply@moj.gov.sa";

    /// <summary>
    /// From name for email sender.
    /// </summary>
    public string FromName { get; set; } = "وزارة العدل - نظام إدارة الدعاوى";

    /// <summary>
    /// SMTP server hostname.
    /// </summary>
    public string? SmtpHost { get; set; }

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP account username.
    /// </summary>
    public string? SmtpUsername { get; set; }

    /// <summary>
    /// SMTP account password.
    /// </summary>
    public string? SmtpPassword { get; set; }

    /// <summary>
    /// Whether to use SSL/TLS for SMTP.
    /// </summary>
    public bool UseSslTls { get; set; } = true;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
