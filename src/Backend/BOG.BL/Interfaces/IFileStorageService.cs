namespace BOG.BL.Interfaces;

/// <summary>
/// Interface for file storage operations.
/// Abstracts physical file persistence to enable testing and storage strategy changes.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves file content to disk storage.
    /// Creates directory if needed.
    /// </summary>
    /// <param name="content">The file content bytes</param>
    /// <param name="fileName">The stored filename (without path)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The full path where file was saved</returns>
    /// <exception cref="IOException">If file I/O fails</exception>
    /// <exception cref="UnauthorizedAccessException">If insufficient permissions</exception>
    Task<string> SaveFileAsync(byte[] content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves file content from disk storage.
    /// </summary>
    /// <param name="storedFileName">The stored filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file content bytes</returns>
    /// <exception cref="FileNotFoundException">If file doesn't exist</exception>
    Task<byte[]> GetFileAsync(string storedFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes physical file from disk storage.
    /// Best-effort: doesn't throw if file doesn't exist.
    /// </summary>
    /// <param name="storedFileName">The stored filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(string storedFileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if file exists on disk.
    /// </summary>
    /// <param name="storedFileName">The stored filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string storedFileName, CancellationToken cancellationToken = default);
}
