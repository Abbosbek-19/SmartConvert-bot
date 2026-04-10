using Microsoft.Extensions.Logging;
using TelegramConverterBot.Models;

namespace TelegramConverterBot.Services;

/// <summary>
/// Detects file types from file extensions and MIME types.
/// </summary>
public class FileDetectorService
{
    private readonly ILogger<FileDetectorService> _logger;

    /// <summary>
    /// Mapping of file extensions to FileType enum values.
    /// </summary>
    private static readonly Dictionary<string, FileType> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".docx", FileType.Docx },
        { ".pdf", FileType.Pdf },
        { ".pptx", FileType.Pptx },
        { ".xlsx", FileType.Xlsx },
        { ".jpg", FileType.Image },
        { ".jpeg", FileType.Image },
        { ".png", FileType.Image }
    };

    /// <summary>
    /// Mapping of MIME types to FileType enum values.
    /// </summary>
    private static readonly Dictionary<string, FileType> MimeTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileType.Docx },
        { "application/pdf", FileType.Pdf },
        { "application/vnd.openxmlformats-officedocument.presentationml.presentation", FileType.Pptx },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileType.Xlsx },
        { "image/jpeg", FileType.Image },
        { "image/png", FileType.Image },
        { "image/jpg", FileType.Image },
        { "image/*", FileType.Image }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDetectorService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public FileDetectorService(ILogger<FileDetectorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Detects the file type from the file name and optional MIME type.
    /// </summary>
    /// <param name="fileName">The file name including extension.</param>
    /// <param name="mimeType">Optional MIME type string from Telegram.</param>
    /// <returns>The detected FileType enum value.</returns>
    public FileType DetectFileType(string? fileName, string? mimeType = null)
    {
        // First try by MIME type if available
        if (!string.IsNullOrWhiteSpace(mimeType))
        {
            if (MimeTypeMap.TryGetValue(mimeType, out var fileTypeFromMime))
            {
                _logger.LogDebug("Detected file type {FileType} from MIME type {MimeType}", fileTypeFromMime, mimeType);
                return fileTypeFromMime;
            }

            // Handle wildcard MIME types like image/*
            if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Detected file type Image from MIME type pattern {MimeType}", mimeType);
                return FileType.Image;
            }
        }

        // Fall back to extension detection
        if (string.IsNullOrWhiteSpace(fileName))
        {
            _logger.LogWarning("Cannot detect file type: file name is null or empty");
            return FileType.Unknown;
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
        {
            _logger.LogWarning("Cannot detect file type: no extension found in {FileName}", fileName);
            return FileType.Unknown;
        }

        if (ExtensionMap.TryGetValue(extension, out var fileType))
        {
            _logger.LogDebug("Detected file type {FileType} from extension {Extension}", fileType, extension);
            return fileType;
        }

        _logger.LogWarning("Unknown file type for extension {Extension}", extension);
        return FileType.Unknown;
    }
}
