using System.Collections.Concurrent;
using TelegramConverterBot.Localization;
using TelegramConverterBot.Models;

namespace TelegramConverterBot.Services;

/// <summary>
/// Manages per-user language preferences and provides localized strings.
/// </summary>
public class LocalizationService
{
    /// <summary>
    /// Stores each user's preferred language, keyed by chatId.
    /// </summary>
    private static readonly ConcurrentDictionary<long, UserLanguage> _userLanguages = new();

    /// <summary>
    /// Gets the language for a user. Defaults to English if not set.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>The user's preferred language.</returns>
    public UserLanguage GetLanguage(long chatId)
    {
        return _userLanguages.GetValueOrDefault(chatId, UserLanguage.En);
    }

    /// <summary>
    /// Sets the language for a user.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="language">The language to set.</param>
    public void SetLanguage(long chatId, UserLanguage language)
    {
        _userLanguages[chatId] = language;
    }

    /// <summary>
    /// Removes the language preference for a user (e.g., on cleanup).
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    public void RemoveLanguage(long chatId)
    {
        _userLanguages.TryRemove(chatId, out _);
    }

    /// <summary>
    /// Gets a localized string value for a user's language.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="en">The English string value.</param>
    /// <param name="uz">The Uzbek string value.</param>
    /// <param name="ru">The Russian string value.</param>
    /// <returns>The localized string for the user's language.</returns>
    public string GetString(long chatId, string en, string uz, string ru)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => uz,
            UserLanguage.Ru => ru,
            _ => en
        };
    }

    /// <summary>
    /// Gets localized welcome message.
    /// </summary>
    public string GetWelcomeMessage(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.WelcomeMessage,
            UserLanguage.Ru => Ru.WelcomeMessage,
            _ => En.WelcomeMessage
        };
    }

    /// <summary>
    /// Gets localized "file too large" message.
    /// </summary>
    public string GetFileTooLarge(long chatId, int maxMB)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => string.Format(Uz.FileTooLarge, maxMB),
            UserLanguage.Ru => string.Format(Ru.FileTooLarge, maxMB),
            _ => string.Format(En.FileTooLarge, maxMB)
        };
    }

    /// <summary>
    /// Gets localized unsupported file type message.
    /// </summary>
    public string GetUnsupportedFileType(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.UnsupportedFileType,
            UserLanguage.Ru => Ru.UnsupportedFileType,
            _ => En.UnsupportedFileType
        };
    }

    /// <summary>
    /// Gets localized "no file uploaded" message.
    /// </summary>
    public string GetNoFileUploaded(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.NoFileUploaded,
            UserLanguage.Ru => Ru.NoFileUploaded,
            _ => En.NoFileUploaded
        };
    }

    /// <summary>
    /// Gets localized download failed message.
    /// </summary>
    public string GetDownloadFailed(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.DownloadFailed,
            UserLanguage.Ru => Ru.DownloadFailed,
            _ => En.DownloadFailed
        };
    }

    /// <summary>
    /// Gets localized conversion failed message.
    /// </summary>
    public string GetConversionFailed(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.ConversionFailed,
            UserLanguage.Ru => Ru.ConversionFailed,
            _ => En.ConversionFailed
        };
    }

    /// <summary>
    /// Gets localized conversion cancelled message.
    /// </summary>
    public string GetConversionCancelled(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.ConversionCancelled,
            UserLanguage.Ru => Ru.ConversionCancelled,
            _ => En.ConversionCancelled
        };
    }

    /// <summary>
    /// Gets localized converting message.
    /// </summary>
    public string GetConverting(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.Converting,
            UserLanguage.Ru => Ru.Converting,
            _ => En.Converting
        };
    }

    /// <summary>
    /// Gets localized done message.
    /// </summary>
    public string GetDone(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.Done,
            UserLanguage.Ru => Ru.Done,
            _ => En.Done
        };
    }

    /// <summary>
    /// Gets localized session expired message.
    /// </summary>
    public string GetSessionExpired(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.SessionExpired,
            UserLanguage.Ru => Ru.SessionExpired,
            _ => En.SessionExpired
        };
    }

    /// <summary>
    /// Gets localized conversion mismatch message.
    /// </summary>
    public string GetConversionMismatch(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.ConversionMismatch,
            UserLanguage.Ru => Ru.ConversionMismatch,
            _ => En.ConversionMismatch
        };
    }

    /// <summary>
    /// Gets localized invalid conversion message.
    /// </summary>
    public string GetInvalidConversion(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.InvalidConversion,
            UserLanguage.Ru => Ru.InvalidConversion,
            _ => En.InvalidConversion
        };
    }

    /// <summary>
    /// Gets localized unknown command message.
    /// </summary>
    public string GetUnknownCommand(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.UnknownCommand,
            UserLanguage.Ru => Ru.UnknownCommand,
            _ => En.UnknownCommand
        };
    }

    /// <summary>
    /// Gets localized language changed message.
    /// </summary>
    public string GetLanguageChanged(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.LanguageChanged,
            UserLanguage.Ru => Ru.LanguageChanged,
            _ => En.LanguageChanged
        };
    }

    /// <summary>
    /// Gets localized detected file message.
    /// </summary>
    public string GetDetectedFile(long chatId, string emoji, string fileType)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => string.Format(Uz.DetectedFile, emoji, fileType),
            UserLanguage.Ru => string.Format(Ru.DetectedFile, emoji, fileType),
            _ => string.Format(En.DetectedFile, emoji, fileType)
        };
    }

    /// <summary>
    /// Gets localized select language message.
    /// </summary>
    public string GetSelectLanguage(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.SelectLanguage,
            UserLanguage.Ru => Ru.SelectLanguage,
            _ => En.SelectLanguage
        };
    }

    /// <summary>
    /// Gets localized DOCX → PDF button text.
    /// </summary>
    public string GetBtnDocxToPdf(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnDocxToPdf,
            UserLanguage.Ru => Ru.BtnDocxToPdf,
            _ => En.BtnDocxToPdf
        };
    }

    /// <summary>
    /// Gets localized PDF → DOCX button text.
    /// </summary>
    public string GetBtnPdfToDocx(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnPdfToDocx,
            UserLanguage.Ru => Ru.BtnPdfToDocx,
            _ => En.BtnPdfToDocx
        };
    }

    /// <summary>
    /// Gets localized PPTX → PDF button text.
    /// </summary>
    public string GetBtnPptxToPdf(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnPptxToPdf,
            UserLanguage.Ru => Ru.BtnPptxToPdf,
            _ => En.BtnPptxToPdf
        };
    }

    /// <summary>
    /// Gets localized XLSX → PDF button text.
    /// </summary>
    public string GetBtnXlsxToPdf(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnXlsxToPdf,
            UserLanguage.Ru => Ru.BtnXlsxToPdf,
            _ => En.BtnXlsxToPdf
        };
    }

    /// <summary>
    /// Gets localized Image → PDF button text.
    /// </summary>
    public string GetBtnImageToPdf(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnImageToPdf,
            UserLanguage.Ru => Ru.BtnImageToPdf,
            _ => En.BtnImageToPdf
        };
    }

    /// <summary>
    /// Gets localized language button text.
    /// </summary>
    public string GetBtnLanguage(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnLanguage,
            UserLanguage.Ru => Ru.BtnLanguage,
            _ => En.BtnLanguage
        };
    }

    /// <summary>
    /// Gets localized English button text.
    /// </summary>
    public string GetBtnEnglish(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnEnglish,
            UserLanguage.Ru => Ru.BtnEnglish,
            _ => En.BtnEnglish
        };
    }

    /// <summary>
    /// Gets localized Uzbek button text.
    /// </summary>
    public string GetBtnUzbek(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnUzbek,
            UserLanguage.Ru => Ru.BtnUzbek,
            _ => En.BtnUzbek
        };
    }

    /// <summary>
    /// Gets localized Russian button text.
    /// </summary>
    public string GetBtnRussian(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.BtnRussian,
            UserLanguage.Ru => Ru.BtnRussian,
            _ => En.BtnRussian
        };
    }

    // Admin panel methods

    /// <summary>
    /// Gets localized admin welcome message.
    /// </summary>
    public string GetAdminWelcome(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminWelcome,
            UserLanguage.Ru => Ru.AdminWelcome,
            _ => En.AdminWelcome
        };
    }

    /// <summary>
    /// Gets localized admin stats button text.
    /// </summary>
    public string GetAdminStats(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminStats,
            UserLanguage.Ru => Ru.AdminStats,
            _ => En.AdminStats
        };
    }

    /// <summary>
    /// Gets localized admin users button text.
    /// </summary>
    public string GetAdminUsers(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminUsers,
            UserLanguage.Ru => Ru.AdminUsers,
            _ => En.AdminUsers
        };
    }

    /// <summary>
    /// Gets localized admin broadcast button text.
    /// </summary>
    public string GetAdminBroadcast(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminBroadcast,
            UserLanguage.Ru => Ru.AdminBroadcast,
            _ => En.AdminBroadcast
        };
    }

    /// <summary>
    /// Gets localized admin reset stats button text.
    /// </summary>
    public string GetAdminResetStats(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminResetStats,
            UserLanguage.Ru => Ru.AdminResetStats,
            _ => En.AdminResetStats
        };
    }

    /// <summary>
    /// Gets localized admin not authorized message.
    /// </summary>
    public string GetAdminNotAuthorized(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminNotAuthorized,
            UserLanguage.Ru => Ru.AdminNotAuthorized,
            _ => En.AdminNotAuthorized
        };
    }

    /// <summary>
    /// Gets localized admin broadcast prompt message.
    /// </summary>
    public string GetAdminBroadcastPrompt(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminBroadcastPrompt,
            UserLanguage.Ru => Ru.AdminBroadcastPrompt,
            _ => En.AdminBroadcastPrompt
        };
    }

    /// <summary>
    /// Gets localized admin broadcast cancel message.
    /// </summary>
    public string GetAdminBroadcastCancel(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminBroadcastCancel,
            UserLanguage.Ru => Ru.AdminBroadcastCancel,
            _ => En.AdminBroadcastCancel
        };
    }

    /// <summary>
    /// Gets localized admin stats reset message.
    /// </summary>
    public string GetAdminStatsReset(long chatId)
    {
        return GetLanguage(chatId) switch
        {
            UserLanguage.Uz => Uz.AdminStatsReset,
            UserLanguage.Ru => Ru.AdminStatsReset,
            _ => En.AdminStatsReset
        };
    }
}
