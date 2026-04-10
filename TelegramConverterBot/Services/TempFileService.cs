using Microsoft.Extensions.Logging;

namespace TelegramConverterBot.Services;

/// <summary>
/// Manages temporary file storage and cleanup for conversion jobs.
/// </summary>
public class TempFileService
{
    private readonly string _basePath;
    private readonly ILogger<TempFileService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TempFileService"/> class.
    /// </summary>
    /// <param name="basePath">The base path for temporary files (from configuration).</param>
    /// <param name="logger">The logger instance.</param>
    public TempFileService(string basePath, ILogger<TempFileService> logger)
    {
        _basePath = Path.GetFullPath(basePath);
        _logger = logger;

        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created temp base directory: {BasePath}", _basePath);
        }
    }

    /// <summary>
    /// Creates a temporary directory for a specific chat session.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID to create a directory for.</param>
    /// <returns>The full path to the created temporary directory.</returns>
    public string CreateTempDirectory(long chatId)
    {
        var tempDir = Path.Combine(_basePath, chatId.ToString());

        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
            _logger.LogDebug("Created temp directory for chat {ChatId}: {TempDir}", chatId, tempDir);
        }

        return tempDir;
    }

    /// <summary>
    /// Gets a full temporary file path for a given file name within a chat's temp directory.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="fileName">The original file name.</param>
    /// <returns>A full path to use for the temporary file.</returns>
    public string GetTempPath(long chatId, string fileName)
    {
        var tempDir = CreateTempDirectory(chatId);
        var sanitizedName = MakeSafeFileName(fileName);
        return Path.Combine(tempDir, sanitizedName);
    }

    /// <summary>
    /// Cleans up all temporary files for a specific chat session.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID to clean up.</param>
    public void CleanupJob(long chatId)
    {
        var tempDir = Path.Combine(_basePath, chatId.ToString());

        try
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
                _logger.LogInformation("Cleaned up temp files for chat {ChatId}", chatId);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
        {
            _logger.LogWarning(ex, "Failed to cleanup temp files for chat {ChatId}", chatId);
        }
    }

    /// <summary>
    /// Deletes a specific temporary file.
    /// </summary>
    /// <param name="filePath">The full path to the file to delete.</param>
    public void DeleteTempFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogDebug("Deleted temp file: {FilePath}", filePath);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
        {
            _logger.LogWarning(ex, "Failed to delete temp file: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Makes a file name safe for use in the file system by removing invalid characters.
    /// </summary>
    /// <param name="fileName">The original file name.</param>
    /// <returns>A sanitized file name.</returns>
    private static string MakeSafeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
