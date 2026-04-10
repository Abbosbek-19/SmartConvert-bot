namespace TelegramConverterBot.Localization;

/// <summary>
/// Russian language strings for the bot.
/// </summary>
public static class Ru
{
    public const string WelcomeMessage = @"👋 Добро пожаловать в бот конвертации файлов!

Отправьте мне любой из этих файлов, и я его конвертирую:
📄 DOCX → PDF
📕 PDF → DOCX
📊 XLSX → PDF
📽 PPTX → PDF
🖼 Изображение (JPG/PNG) → PDF

Просто отправьте файл, и я сделаю всё остальное!";

    public const string HelpMessage = WelcomeMessage;

    public const string FileTooLarge = "❌ Файл слишком большой. Максимальный размер {0}МБ.";
    public const string UnsupportedFileType = "❌ Извините, этот тип файла не поддерживается.";
    public const string NoFileUploaded = "📎 Пожалуйста, отправьте файл для конвертации.";
    public const string DownloadFailed = "❌ Не удалось загрузить файл. Пожалуйста, попробуйте снова.";
    public const string ConversionFailed = "❌ Конвертация не удалась. Пожалуйста, попробуйте снова.";
    public const string ConversionCancelled = "❌ Конвертация была отменена.";
    public const string Converting = "⏳ Конвертация, пожалуйста, подождите...";
    public const string Done = "✅ Готово! Вот ваш конвертированный файл.";
    public const string SessionExpired = "⚠️ Сессия истекла. Пожалуйста, отправьте файл снова.";
    public const string ConversionMismatch = "⚠️ Несоответствие конвертации. Пожалуйста, отправьте файл снова.";
    public const string InvalidConversion = "⚠️ Неверный запрос конвертации.";
    public const string UnknownCommand = "❓ Неизвестная команда. Используйте /start, чтобы увидеть доступные функции.";
    public const string LanguageChanged = "✅ Язык изменён на русский.";

    public const string DetectedFile = "✅ Обнаружено: {0} {1}\nВыберите конвертацию:";
    public const string SelectLanguage = "🌐 Выберите язык:";

    // Inline keyboard buttons
    public const string BtnDocxToPdf = "📄 DOCX → PDF";
    public const string BtnPdfToDocx = "📕 PDF → DOCX";
    public const string BtnPptxToPdf = "📽 PPTX → PDF";
    public const string BtnXlsxToPdf = "📊 XLSX → PDF";
    public const string BtnImageToPdf = "🖼 Изображение → PDF";

    // Language buttons
    public const string BtnLanguage = "🌐 Изменить язык";
    public const string BtnEnglish = "🇬🇧 English";
    public const string BtnUzbek = "🇺🇿 O'zbekcha";
    public const string BtnRussian = "🇷🇺 Русский";
}
