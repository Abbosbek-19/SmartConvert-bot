using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Font;

namespace TelegramConverterBot.Helpers;

/// <summary>
/// Resolves a Unicode-capable PdfFont by probing well-known system font paths,
/// falling back to the built-in Helvetica when none are available.
/// </summary>
public static class PdfFontHelper
{
    private static readonly string[] CandidateFontPaths =
    {
        // Linux (Docker) — installed via fonts-dejavu-core
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/TTF/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
        "/usr/share/fonts/truetype/noto/NotoSans-Regular.ttf",
        // Windows
        @"C:\Windows\Fonts\arial.ttf",
        @"C:\Windows\Fonts\segoeui.ttf",
        // macOS
        "/Library/Fonts/Arial Unicode.ttf",
        "/System/Library/Fonts/Supplemental/Arial.ttf",
    };

    /// <summary>
    /// Creates a Unicode-capable font (IDENTITY_H) from the first available system font,
    /// or Helvetica when no Unicode font is found.
    /// </summary>
    public static PdfFont CreateUnicodeFont()
    {
        foreach (var path in CandidateFontPaths)
        {
            if (!File.Exists(path)) continue;
            try
            {
                return PdfFontFactory.CreateFont(
                    path,
                    PdfEncodings.IDENTITY_H,
                    PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            }
            catch
            {
                // try next candidate
            }
        }

        return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
    }
}
