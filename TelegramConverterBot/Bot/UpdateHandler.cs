using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Models;
using TelegramConverterBot.Services;
using FileType = TelegramConverterBot.Models.FileType;

namespace TelegramConverterBot.Bot;

/// <summary>
/// Handles all incoming Telegram updates including messages and callback queries.
/// </summary>
public class UpdateHandler
{
    private readonly TelegramBotClient _botClient;
    private readonly FileDetectorService _fileDetector;
    private readonly TempFileService _tempFileService;
    private readonly Helpers.FileHelper _fileHelper;
    private readonly CallbackHandler _callbackHandler;
    private readonly LocalizationService _localizationService;
    private readonly AdminService _adminService;
    private readonly ActivityTracker _activityTracker;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly int _maxFileSizeMB;
    private readonly long[] _adminIds;

    /// <summary>
    /// Stores active conversion jobs per chat session.
    /// </summary>
    public static readonly System.Collections.Concurrent.ConcurrentDictionary<long, ConversionJob> ActiveJobs = new();

    /// <summary>
    /// Stores chat IDs that are waiting for broadcast messages.
    /// </summary>
    public static readonly System.Collections.Concurrent.ConcurrentDictionary<long, bool> BroadcastWaiting = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateHandler"/> class.
    /// </summary>
    public UpdateHandler(
        TelegramBotClient botClient,
        FileDetectorService fileDetector,
        TempFileService tempFileService,
        Helpers.FileHelper fileHelper,
        CallbackHandler callbackHandler,
        LocalizationService localizationService,
        AdminService adminService,
        ActivityTracker activityTracker,
        IOptions<BotConfiguration> config,
        ILogger<UpdateHandler> logger)
    {
        _botClient = botClient;
        _fileDetector = fileDetector;
        _tempFileService = tempFileService;
        _fileHelper = fileHelper;
        _callbackHandler = callbackHandler;
        _localizationService = localizationService;
        _adminService = adminService;
        _activityTracker = activityTracker;
        _logger = logger;
        _maxFileSizeMB = config.Value.MaxFileSizeMB;
        _adminIds = config.Value.AdminIds;
    }

    /// <summary>
    /// Handles an incoming Telegram update.
    /// </summary>
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessageAsync(update.Message!, cancellationToken);
                    break;

                case UpdateType.CallbackQuery:
                    await _callbackHandler.HandleCallbackAsync(update.CallbackQuery!, cancellationToken);
                    break;

                default:
                    _logger.LogDebug("Unhandled update type: {UpdateType}", update.Type);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Update handling was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update of type {UpdateType}", update.Type);
        }
    }

    /// <summary>
    /// Handles errors that occur during update processing.
    /// </summary>
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled error from source {Source}: {ExceptionType}", source, exception.GetType().Name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles incoming message updates.
    /// </summary>
    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        // Track user activity
        _activityTracker.RecordUserActivity(
            chatId,
            message.From?.Username,
            message.From?.FirstName,
            message.From?.LastName,
            _localizationService.GetLanguage(chatId));

        // Check if admin is waiting for broadcast message
        if (BroadcastWaiting.ContainsKey(chatId) && message.Type == MessageType.Text)
        {
            await HandleBroadcastMessageAsync(chatId, message.Text!, cancellationToken);
            return;
        }

        // Handle commands
        if (message.Type == MessageType.Text && message.Text!.StartsWith('/'))
        {
            await HandleCommandAsync(message, cancellationToken);
            return;
        }

        // Check if message contains a document
        if (message.Document != null)
        {
            await HandleDocumentAsync(message, cancellationToken);
            return;
        }

        // Check if message contains a photo
        if (message.Photo != null && message.Photo.Length > 0)
        {
            await HandlePhotoAsync(message, cancellationToken);
            return;
        }

        // No file detected
        await _botClient.SendMessage(
            chatId,
            _localizationService.GetNoFileUploaded(chatId),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Handles bot commands like /start, /help, /lang, and /admin.
    /// </summary>
    private async Task HandleCommandAsync(Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var command = message.Text!.Split(' ')[0].ToLowerInvariant();

        // Handle admin commands
        if (command == "/admin")
        {
            if (!_adminService.IsAdmin(chatId))
            {
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetAdminNotAuthorized(chatId),
                    cancellationToken: cancellationToken);
                return;
            }

            var keyboard = KeyboardBuilder.BuildAdminKeyboard(_localizationService, chatId);
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetAdminWelcome(chatId),
                replyMarkup: keyboard,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
            return;
        }

        switch (command)
        {
            case "/start":
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetWelcomeMessage(chatId),
                    cancellationToken: cancellationToken);
                break;

            case "/help":
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetWelcomeMessage(chatId),
                    cancellationToken: cancellationToken);
                break;

            case "/lang":
                var keyboard = KeyboardBuilder.BuildLanguageKeyboard(_localizationService, chatId);
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetSelectLanguage(chatId),
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                break;

            default:
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetUnknownCommand(chatId),
                    cancellationToken: cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Handles document file uploads (generic documents with MIME type).
    /// </summary>
    private async Task HandleDocumentAsync(Message message, CancellationToken cancellationToken)
    {
        var document = message.Document!;
        var chatId = message.Chat.Id;
        var fileName = document.FileName ?? "document.docx";
        var mimeType = document.MimeType;

        await ProcessFileAsync(chatId, document.FileId, fileName, mimeType, cancellationToken);
    }

    /// <summary>
    /// Handles broadcast message input from admin.
    /// </summary>
    private async Task HandleBroadcastMessageAsync(long chatId, string message, CancellationToken cancellationToken)
    {
        if (message == "/cancel")
        {
            BroadcastWaiting.TryRemove(chatId, out _);
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetAdminBroadcastCancel(chatId),
                cancellationToken: cancellationToken);
            return;
        }

        BroadcastWaiting.TryRemove(chatId, out _);

        var statusMsg = await _botClient.SendMessage(
            chatId,
            "📤 Broadcasting message...",
            cancellationToken: cancellationToken);

        var result = await _adminService.BroadcastMessageAsync(message, cancellationToken);
        var resultMessage = _adminService.GetBroadcastResultMessage(result);

        await _botClient.EditMessageText(
            chatId,
            statusMsg.MessageId,
            resultMessage,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Handles photo file uploads.
    /// </summary>
    private async Task HandlePhotoAsync(Message message, CancellationToken cancellationToken)
    {
        var photo = message.Photo!.Last()!; // Highest resolution
        var chatId = message.Chat.Id;
        var fileName = $"photo_{message.MessageId}.jpg";
        var mimeType = "image/jpeg";

        await ProcessFileAsync(chatId, photo.FileId, fileName, mimeType, cancellationToken);
    }

    /// <summary>
    /// Processes an uploaded file: validates, detects type, saves temp, and shows conversion options.
    /// </summary>
    private async Task ProcessFileAsync(long chatId, string fileId, string fileName, string? mimeType, CancellationToken cancellationToken)
    {
        // Clean up any existing job for this chat
        ActiveJobs.TryRemove(chatId, out _);

        // Create temp directory and path
        var tempDir = _tempFileService.CreateTempDirectory(chatId);
        var sanitizedName = Helpers.FileHelper.SanitizeFileName(fileName, "document");
        var tempPath = _tempFileService.GetTempPath(chatId, sanitizedName);

        // Download file
        try
        {
            await _fileHelper.DownloadFileAsync(fileId, tempPath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {FileId} for chat {ChatId}", fileId, chatId);
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetDownloadFailed(chatId),
                cancellationToken: cancellationToken);
            return;
        }

        // Check file size
        var fileInfo = new FileInfo(tempPath);
        var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

        if (fileSizeMB > _maxFileSizeMB)
        {
            _tempFileService.DeleteTempFile(tempPath);
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetFileTooLarge(chatId, _maxFileSizeMB),
                cancellationToken: cancellationToken);
            return;
        }

        // Detect file type
        var fileType = _fileDetector.DetectFileType(sanitizedName, mimeType);

        if (fileType == FileType.Unknown)
        {
            _tempFileService.CleanupJob(chatId);
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetUnsupportedFileType(chatId),
                cancellationToken: cancellationToken);
            return;
        }

        // Create conversion job
        var job = new ConversionJob
        {
            ChatId = chatId,
            OriginalFileName = sanitizedName,
            TempInputPath = tempPath,
            DetectedType = fileType,
            CreatedAt = DateTime.UtcNow
        };

        ActiveJobs[chatId] = job;

        // Show conversion options with localized buttons
        var keyboard = KeyboardBuilder.BuildConversionKeyboard(fileType, _localizationService, chatId);

        var emoji = fileType switch
        {
            FileType.Docx => "📄",
            FileType.Pdf => "📕",
            FileType.Pptx => "📽",
            FileType.Xlsx => "📊",
            FileType.Image => "🖼",
            _ => "📎"
        };

        await _botClient.SendMessage(
            chatId,
            _localizationService.GetDetectedFile(chatId, emoji, fileType.ToString()),
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Configuration class for bot settings.
/// </summary>
public class BotConfiguration
{
    /// <summary>
    /// The Telegram bot token.
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// The path for storing temporary files.
    /// </summary>
    public string TempFilesPath { get; set; } = "temp/";

    /// <summary>
    /// Maximum allowed file size in megabytes.
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 20;

    /// <summary>
    /// Array of Telegram user IDs who are administrators.
    /// </summary>
    public long[] AdminIds { get; set; } = Array.Empty<long>();
}
