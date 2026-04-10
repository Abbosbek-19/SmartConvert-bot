using TelegramConverterBot.Models;

namespace TelegramConverterBot.Models;

/// <summary>
/// Represents a file conversion job with all necessary metadata.
/// </summary>
public class ConversionJob
{
    /// <summary>
    /// The Telegram chat ID associated with this conversion job.
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// The original file name as provided by the user.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// The full path to the temporary input file.
    /// </summary>
    public string TempInputPath { get; set; } = string.Empty;

    /// <summary>
    /// The detected file type of the uploaded file.
    /// </summary>
    public FileType DetectedType { get; set; }

    /// <summary>
    /// The time when this job was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
