namespace TelegramConverterBot.Localization;

/// <summary>
/// English language strings for the bot.
/// </summary>
public static class En
{
    public const string WelcomeMessage = @"👋 Welcome to File Converter Bot!

Send me any of these files and I'll convert it:
📄 DOCX → PDF
📕 PDF → DOCX
📊 XLSX → PDF
📽 PPTX → PDF
🖼 Image (JPG/PNG) → PDF

Just send your file and I'll do the rest!";

    public const string HelpMessage = WelcomeMessage;

    public const string FileTooLarge = "❌ File too large. Max size is {0}MB.";
    public const string UnsupportedFileType = "❌ Sorry, this file type is not supported.";
    public const string NoFileUploaded = "📎 Please send a file to convert.";
    public const string DownloadFailed = "❌ Failed to download file. Please try again.";
    public const string ConversionFailed = "❌ Conversion failed. Please try again.";
    public const string ConversionCancelled = "❌ Conversion was cancelled.";
    public const string Converting = "⏳ Converting, please wait...";
    public const string Done = "✅ Done! Here is your converted file.";
    public const string SessionExpired = "⚠️ Session expired. Please send your file again.";
    public const string ConversionMismatch = "⚠️ Conversion mismatch. Please send your file again.";
    public const string InvalidConversion = "⚠️ Invalid conversion request.";
    public const string UnknownCommand = "❓ Unknown command. Use /start to see available features.";
    public const string LanguageChanged = "✅ Language changed to English.";

    public const string DetectedFile = "✅ Detected: {0} {1}\nChoose conversion:";
    public const string SelectLanguage = "🌐 Select your language:";

    // Inline keyboard buttons
    public const string BtnDocxToPdf = "📄 DOCX → PDF";
    public const string BtnPdfToDocx = "📕 PDF → DOCX";
    public const string BtnPptxToPdf = "📽 PPTX → PDF";
    public const string BtnXlsxToPdf = "📊 XLSX → PDF";
    public const string BtnImageToPdf = "🖼 Image → PDF";

    // Language buttons
    public const string BtnLanguage = "🌐 Change Language";
    public const string BtnEnglish = "🇬🇧 English";
    public const string BtnUzbek = "🇺🇿 O'zbekcha";
    public const string BtnRussian = "🇷🇺 Русский";

    // Admin panel
    public const string AdminWelcome = @"⚙️ **Admin Panel**

Choose an option:";
    public const string AdminStats = "📊 Statistics";
    public const string AdminUsers = "👥 Users";
    public const string AdminBroadcast = "📢 Broadcast";
    public const string AdminResetStats = "🔄 Reset Stats";
    public const string AdminNotAuthorized = "❌ You are not authorized to use this command.";
    public const string AdminBroadcastPrompt = "📝 Send the message you want to broadcast to all users:";
    public const string AdminBroadcastCancel = "❌ Broadcast cancelled.";
    public const string AdminStatsReset = "✅ Statistics have been reset.";
}
