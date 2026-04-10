namespace TelegramConverterBot.Models;

/// <summary>
/// Represents supported source file types.
/// </summary>
public enum FileType
{
    /// <summary>
    /// Unknown or unsupported file type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Microsoft Word document (.docx).
    /// </summary>
    Docx,

    /// <summary>
    /// Portable Document Format (.pdf).
    /// </summary>
    Pdf,

    /// <summary>
    /// Microsoft PowerPoint presentation (.pptx).
    /// </summary>
    Pptx,

    /// <summary>
    /// Microsoft Excel workbook (.xlsx).
    /// </summary>
    Xlsx,

    /// <summary>
    /// Image file (.jpg, .jpeg, .png).
    /// </summary>
    Image
}
