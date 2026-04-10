using Telegram.Bot.Types.ReplyMarkups;
using TelegramConverterBot.Models;
using TelegramConverterBot.Services;

namespace TelegramConverterBot.Helpers;

/// <summary>
/// Builds inline keyboard markup for Telegram bot messages based on detected file type.
/// </summary>
public static class KeyboardBuilder
{
    /// <summary>
    /// Creates an inline keyboard with conversion options based on the detected file type.
    /// </summary>
    /// <param name="fileType">The detected source file type.</param>
    /// <param name="localizationService">The localization service for button text.</param>
    /// <param name="chatId">The chat ID for language preference lookup.</param>
    /// <returns>An InlineKeyboardMarkup with appropriate conversion buttons.</returns>
    public static InlineKeyboardMarkup BuildConversionKeyboard(
        FileType fileType,
        LocalizationService localizationService,
        long chatId)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        switch (fileType)
        {
            case FileType.Docx:
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        localizationService.GetBtnDocxToPdf(chatId),
                        BuildCallbackData(FileType.Docx, ConversionTarget.Pdf))
                });
                break;

            case FileType.Pdf:
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        localizationService.GetBtnPdfToDocx(chatId),
                        BuildCallbackData(FileType.Pdf, ConversionTarget.Docx))
                });
                break;

            case FileType.Pptx:
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        localizationService.GetBtnPptxToPdf(chatId),
                        BuildCallbackData(FileType.Pptx, ConversionTarget.Pdf))
                });
                break;

            case FileType.Xlsx:
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        localizationService.GetBtnXlsxToPdf(chatId),
                        BuildCallbackData(FileType.Xlsx, ConversionTarget.Pdf))
                });
                break;

            case FileType.Image:
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        localizationService.GetBtnImageToPdf(chatId),
                        BuildCallbackData(FileType.Image, ConversionTarget.Pdf))
                });
                break;

            default:
                // No buttons for unknown types
                break;
        }

        // Add language change button at the bottom
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(
                localizationService.GetBtnLanguage(chatId),
                "lang:select")
        });

        return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Creates an inline keyboard for language selection.
    /// </summary>
    /// <param name="localizationService">The localization service for button text.</param>
    /// <param name="chatId">The chat ID for language preference lookup.</param>
    /// <returns>An InlineKeyboardMarkup with language selection buttons.</returns>
    public static InlineKeyboardMarkup BuildLanguageKeyboard(
        LocalizationService localizationService,
        long chatId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    localizationService.GetBtnEnglish(chatId),
                    "lang:set:en"),
                InlineKeyboardButton.WithCallbackData(
                    localizationService.GetBtnUzbek(chatId),
                    "lang:set:uz"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    localizationService.GetBtnRussian(chatId),
                    "lang:set:ru")
            }
        });
    }

    /// <summary>
    /// Builds the callback data string for a conversion action.
    /// </summary>
    /// <param name="source">The source file type.</param>
    /// <param name="target">The target conversion format.</param>
    /// <returns>A formatted callback data string.</returns>
    private static string BuildCallbackData(FileType source, ConversionTarget target)
    {
        return $"convert:{source.ToString().ToLowerInvariant()}:{target.ToString().ToLowerInvariant()}";
    }

    /// <summary>
    /// Parses callback data into source and target formats.
    /// </summary>
    /// <param name="callbackData">The callback data string (format: "convert:source:target").</param>
    /// <param name="source">When this method returns, contains the parsed source file type.</param>
    /// <param name="target">When this method returns, contains the parsed target format.</param>
    /// <returns>True if parsing was successful; otherwise, false.</returns>
    public static bool ParseCallbackData(string callbackData, out FileType source, out ConversionTarget target)
    {
        source = FileType.Unknown;
        target = ConversionTarget.Unknown;

        var parts = callbackData.Split(':');
        if (parts.Length != 3 || !parts[0].Equals("convert", StringComparison.OrdinalIgnoreCase))
            return false;

        Enum.TryParse<FileType>(parts[1], true, out source);
        Enum.TryParse<ConversionTarget>(parts[2], true, out target);

        return source != FileType.Unknown && target != ConversionTarget.Unknown;
    }

    /// <summary>
    /// Parses a language callback.
    /// </summary>
    /// <param name="callbackData">The callback data (format: "lang:set:code").</param>
    /// <param name="language">When this method returns, contains the parsed language code.</param>
    /// <returns>True if this is a valid language callback.</returns>
    public static bool TryParseLanguageCallback(string callbackData, out UserLanguage language)
    {
        language = UserLanguage.En;

        var parts = callbackData.Split(':');
        if (parts.Length != 3 || !parts[0].Equals("lang", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!parts[1].Equals("set", StringComparison.OrdinalIgnoreCase))
            return false;

        return Enum.TryParse<UserLanguage>(parts[2], true, out language);
    }

    /// <summary>
    /// Creates an inline keyboard for the admin panel.
    /// </summary>
    public static InlineKeyboardMarkup BuildAdminKeyboard(LocalizationService localizationService, long chatId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    localizationService.GetAdminStats(chatId),
                    "admin:stats"),
                InlineKeyboardButton.WithCallbackData(
                    localizationService.GetAdminUsers(chatId),
                    "admin:users")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    localizationService.GetAdminBroadcast(chatId),
                    "admin:broadcast"),
                InlineKeyboardButton.WithCallbackData(
                    localizationService.GetAdminResetStats(chatId),
                    "admin:reset")
            }
        });
    }

    /// <summary>
    /// Creates a pagination keyboard for user list navigation.
    /// </summary>
    public static InlineKeyboardMarkup BuildUserPaginationKeyboard(int currentPage, int totalPages)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        var navRow = new List<InlineKeyboardButton>();

        if (currentPage > 1)
        {
            navRow.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"admin:users:page:{currentPage - 1}"));
        }

        navRow.Add(InlineKeyboardButton.WithCallbackData(
            $"📄 {currentPage}/{totalPages}",
            "admin:users:info"));

        if (currentPage < totalPages)
        {
            navRow.Add(InlineKeyboardButton.WithCallbackData("➡️", $"admin:users:page:{currentPage + 1}"));
        }

        buttons.Add(navRow.ToArray());

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔙 Back to Admin", "admin:back")
        });

        return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Parses admin callback data.
    /// </summary>
    public static bool TryParseAdminCallback(string callbackData, out string action, out string? parameter)
    {
        action = string.Empty;
        parameter = null;

        if (!callbackData.StartsWith("admin:", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = callbackData.Split(':');
        if (parts.Length < 2)
            return false;

        action = parts[1];
        if (parts.Length > 2)
        {
            parameter = string.Join(":", parts.Skip(2));
        }

        return true;
    }
}
