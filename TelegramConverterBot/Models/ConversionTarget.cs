namespace TelegramConverterBot.Models;

/// <summary>
/// Represents all possible target formats for conversion.
/// </summary>
public enum ConversionTarget
{
    /// <summary>
    /// Unknown target format.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Portable Document Format (.pdf).
    /// </summary>
    Pdf,

    /// <summary>
    /// Microsoft Word document (.docx).
    /// </summary>
    Docx
}
