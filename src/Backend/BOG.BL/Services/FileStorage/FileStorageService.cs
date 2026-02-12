using BOG.BL.Configuration;
using BOG.BL.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BOG.BL.Services.FileStorage;

/// <summary>
/// File storage service implementation.
/// Persists uploaded files to disk with path traversal protection and error handling.
/// Follows Single Responsibility Principle - only handles file I/O.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _uploadDirectory;

    public FileStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<FileStorageService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Resolve upload path relative to current working directory (or use absolute path if provided)
        _uploadDirectory = Path.IsPathRooted(_settings.UploadPath)
            ? _settings.UploadPath
            : Path.Combine(AppContext.BaseDirectory, _settings.UploadPath);

        // Ensure directory exists if configured
        if (_settings.EnsureDirectoryExists && !Directory.Exists(_uploadDirectory))
        {
            try
            {
                Directory.CreateDirectory(_uploadDirectory);
                _logger.LogInformation("Created upload directory at {UploadDirectory}", _uploadDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create upload directory {UploadDirectory}", _uploadDirectory);
                throw new InvalidOperationException(
                    $"Failed to initialize file storage: cannot create upload directory.", ex);
            }
        }
    }

    public async Task<string> SaveFileAsync(byte[] content, string fileName, CancellationToken cancellationToken = default)
    {
        if (content == null || content.Length == 0)
            throw new ArgumentException("File content cannot be empty.", nameof(content));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));

        // Path traversal protection: strip directory components
        var safeFileName = Path.GetFileName(fileName);

        // Validate normalized path is within upload directory
        var filePath = Path.Combine(_uploadDirectory, safeFileName);
        var fullPath = Path.GetFullPath(filePath);

        if (!fullPath.StartsWith(Path.GetFullPath(_uploadDirectory), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt detected: {FileName}", fileName);
            throw new InvalidOperationException("Invalid file path.");
        }

        try
        {
            // Write file asynchronously
            await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
            _logger.LogInformation("File saved successfully: {FilePath}, Size: {FileSize} bytes", fullPath, content.Length);
            return fullPath;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error while saving file {FileName}", fileName);
            throw new InvalidOperationException(
                "Failed to save attachment file. The file system may be full or inaccessible.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied while saving file {FileName}", fileName);
            throw new InvalidOperationException(
                "Failed to save attachment file due to insufficient permissions.", ex);
        }
    }

    public async Task<byte[]> GetFileAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new ArgumentException("File name cannot be empty.", nameof(storedFileName));

        // Path traversal protection: strip directory components
        var safeFileName = Path.GetFileName(storedFileName);

        // Validate normalized path is within upload directory
        var filePath = Path.Combine(_uploadDirectory, safeFileName);
        var fullPath = Path.GetFullPath(filePath);

        if (!fullPath.StartsWith(Path.GetFullPath(_uploadDirectory), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt detected on download: {FileName}", storedFileName);
            throw new InvalidOperationException("Invalid file path.");
        }

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File not found: {FilePath}", fullPath);
            throw new FileNotFoundException($"The attachment file '{safeFileName}' was not found on the server.");
        }

        try
        {
            var fileContent = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            _logger.LogInformation("File retrieved successfully: {FilePath}, Size: {FileSize} bytes", fullPath, fileContent.Length);
            return fileContent;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error while reading file {FilePath}", fullPath);
            throw new InvalidOperationException(
                "Failed to read attachment file. The file may be corrupted or inaccessible.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied while reading file {FilePath}", fullPath);
            throw new InvalidOperationException(
                "Failed to read attachment file due to insufficient permissions.", ex);
        }
    }

    public async Task DeleteFileAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            return; // Silent return for empty filename

        try
        {
            // Path traversal protection: strip directory components
            var safeFileName = Path.GetFileName(storedFileName);

            // Validate normalized path is within upload directory
            var filePath = Path.Combine(_uploadDirectory, safeFileName);
            var fullPath = Path.GetFullPath(filePath);

            if (!fullPath.StartsWith(Path.GetFullPath(_uploadDirectory), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt detected on delete: {FileName}", storedFileName);
                return; // Silent return on invalid path
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", fullPath);
            }
            else
            {
                _logger.LogDebug("Attempted to delete non-existent file: {FilePath}", fullPath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "IO error while deleting file {FileName}", storedFileName);
            // Best-effort: don't throw, just log
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied while deleting file {FileName}", storedFileName);
            // Best-effort: don't throw, just log
        }

        await Task.CompletedTask; // For async method signature
    }

    public async Task<bool> FileExistsAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            return false;

        try
        {
            // Path traversal protection: strip directory components
            var safeFileName = Path.GetFileName(storedFileName);

            // Validate normalized path is within upload directory
            var filePath = Path.Combine(_uploadDirectory, safeFileName);
            var fullPath = Path.GetFullPath(filePath);

            if (!fullPath.StartsWith(Path.GetFullPath(_uploadDirectory), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt detected on exists check: {FileName}", storedFileName);
                return false;
            }

            return await Task.FromResult(File.Exists(fullPath));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking if file exists: {FileName}", storedFileName);
            return false;
        }
    }
}
