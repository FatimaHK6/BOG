namespace BOG.BL.Configuration;

/// <summary>
/// Configuration settings for file storage operations.
/// Bindable from appsettings.json via Options pattern.
/// </summary>
public class FileStorageSettings
{
    /// <summary>
    /// The directory path where files are stored (relative to content root).
    /// Default: wwwroot/uploads/attachments
    /// </summary>
    public string UploadPath { get; set; } = "wwwroot/uploads/attachments";

    /// <summary>
    /// Maximum file size in bytes.
    /// Default: 4MB (enforces BR04 business rule)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 4 * 1024 * 1024;

    /// <summary>
    /// Allowed content types for upload.
    /// Default: application/pdf only
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = { "application/pdf" };

    /// <summary>
    /// Whether to automatically create upload directory if it doesn't exist.
    /// Default: true
    /// </summary>
    public bool EnsureDirectoryExists { get; set; } = true;
}
