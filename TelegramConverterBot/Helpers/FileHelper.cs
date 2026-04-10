using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramConverterBot.Bot;

namespace TelegramConverterBot.Helpers;

/// <summary>
/// Provides helper methods for downloading and managing files from Telegram.
/// </summary>
public class FileHelper
{
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<FileHelper> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _botToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileHelper"/> class.
    /// </summary>
    /// <param name="botClient">The Telegram bot client instance.</param>
    /// <param name="config">The bot configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public FileHelper(TelegramBotClient botClient, IOptions<BotConfiguration> config, ILogger<FileHelper> logger)
    {
        _botClient = botClient;
        _logger = logger;
        _httpClient = new HttpClient();
        _botToken = config.Value.BotToken;
    }

    /// <summary>
    /// Downloads a file from Telegram to the specified local path.
    /// </summary>
    /// <param name="fileId">The Telegram file ID to download.</param>
    /// <param name="savePath">The full local path where the file should be saved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DownloadFileAsync(string fileId, string savePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading file {FileId} to {SavePath}", fileId, savePath);

        var file = await _botClient.GetFile(fileId, cancellationToken);
        if (file.FilePath is null)
        {
            throw new InvalidOperationException($"Failed to get file path for file ID: {fileId}");
        }

        var directory = Path.GetDirectoryName(savePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Download file using HttpClient from Telegram file URL
        var fileUrl = $"https://api.telegram.org/file/bot{_botToken}/{file.FilePath}";
        var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("File downloaded successfully to {SavePath}", savePath);
    }

    /// <summary>
    /// Extracts the file name from a Telegram document or photo message.
    /// </summary>
    /// <param name="fileName">The optional file name provided by Telegram.</param>
    /// <param name="defaultName">The default name to use if none is provided.</param>
    /// <returns>A sanitized file name with a proper extension.</returns>
    public static string SanitizeFileName(string? fileName, string defaultName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = defaultName;
        }

        // Remove invalid path characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Truncate if too long (max 200 chars)
        if (sanitizedName.Length > 200)
        {
            var ext = Path.GetExtension(sanitizedName);
            sanitizedName = sanitizedName.Substring(0, 200 - ext.Length) + ext;
        }

        return sanitizedName.Trim();
    }

    /// <summary>
    /// Gets the output file path for a converted file.
    /// </summary>
    /// <param name="baseDirectory">The base directory for the conversion job.</param>
    /// <param name="originalFileName">The original file name.</param>
    /// <param name="targetExtension">The target file extension (e.g., ".pdf", ".docx").</param>
    /// <returns>The full path for the output file.</returns>
    public static string GetOutputFilePath(string baseDirectory, string originalFileName, string targetExtension)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
        return Path.Combine(baseDirectory, $"{nameWithoutExt}_converted{targetExtension}");
    }
}
