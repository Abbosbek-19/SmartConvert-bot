namespace TelegramConverterBot.Localization;

/// <summary>
/// Uzbek language strings for the bot.
/// </summary>
public static class Uz
{
    public const string WelcomeMessage = @"👋 Fayl Konvertor Botiga xush kelibsiz!

Meng quyidagi fayllardan birini yuboring, men uni konvertor qilaman:
📄 DOCX → PDF
📕 PDF → DOCX
📊 XLSX → PDF
📽 PPTX → PDF
🖼 Rasm (JPG/PNG) → PDF

Faylingizni yuboring va men hammasini qilaman!";

    public const string HelpMessage = WelcomeMessage;

    public const string FileTooLarge = "❌ Fayl juda katta. Maksimal hajm {0}MB.";
    public const string UnsupportedFileType = "❌ Kechirasiz, bu fayl turi qo'llab-quvvatlanmaydi.";
    public const string NoFileUploaded = "📎 Iltimos, konvertor qilish uchun fayl yuboring.";
    public const string DownloadFailed = "❌ Faylni yuklab bo'lmadi. Iltimos, qayta urinib ko'ring.";
    public const string ConversionFailed = "❌ Konvertor muvaffaqiyatsiz tugadi. Iltimos, qayta urinib ko'ring.";
    public const string ConversionCancelled = "❌ Konvertor bekor qilindi.";
    public const string Converting = "⏳ Konvertor qilinmoqda, iltimos kuting...";
    public const string Done = "✅ Tayyor! Mana sizning konvertor qilingan faylingiz.";
    public const string SessionExpired = "⚠️ Sessiya muddati tugadi. Iltimos, faylingizni qayta yuboring.";
    public const string ConversionMismatch = "⚠️ Konvertor mos kelmadi. Iltimos, faylingizni qayta yuboring.";
    public const string InvalidConversion = "⚠️ Noto'g'ri konvertor so'rovi.";
    public const string UnknownCommand = "❓ Noma'lum buyruq. Mavjud imkoniyatlarni ko'rish uchun /start dan foydalaning.";
    public const string LanguageChanged = "✅ Til o'zbek tiliga o'zgartirildi.";

    public const string DetectedFile = "✅ Aniqlandi: {0} {1}\nKonvertor turini tanlang:";
    public const string SelectLanguage = "🌐 Tilni tanlang:";

    // Inline keyboard buttons
    public const string BtnDocxToPdf = "📄 DOCX → PDF";
    public const string BtnPdfToDocx = "📕 PDF → DOCX";
    public const string BtnPptxToPdf = "📽 PPTX → PDF";
    public const string BtnXlsxToPdf = "📊 XLSX → PDF";
    public const string BtnImageToPdf = "🖼 Rasm → PDF";

    // Language buttons
    public const string BtnLanguage = "🌐 Tilni o'zgartirish";
    public const string BtnEnglish = "🇬🇧 English";
    public const string BtnUzbek = "🇺🇿 O'zbekcha";
    public const string BtnRussian = "🇷🇺 Русский";

    // Admin panel
    public const string AdminWelcome = @"⚙️ \*Admin paneli\*

Variantni tanlang:";
    public const string AdminStats = "📊 Statistika";
    public const string AdminUsers = "👥 Foydalanuvchilar";
    public const string AdminBroadcast = "📢 Tarqatish";
    public const string AdminResetStats = "🔄 Statistika qayta o'rnatish";
    public const string AdminNotAuthorized = "❌ Sizda bu buyruqdan foydalanish huquqi yo'q.";
    public const string AdminBroadcastPrompt = "📝 Barcha foydalanuvchilarga yubormoqchi bo'lgan xabaringizni yuboring:";
    public const string AdminBroadcastCancel = "❌ Tarqatish bekor qilindi.";
    public const string AdminStatsReset = "✅ Statistika qayta o'rnatildi.";
}
