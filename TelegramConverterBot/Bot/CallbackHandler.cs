using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Models;
using TelegramConverterBot.Services;

namespace TelegramConverterBot.Bot;

/// <summary>
/// Handles callback queries from inline keyboard buttons for file conversion and language selection.
/// </summary>
public class CallbackHandler
{
    private readonly TelegramBotClient _botClient;
    private readonly ConversionService _conversionService;
    private readonly TempFileService _tempFileService;
    private readonly LocalizationService _localizationService;
    private readonly AdminService _adminService;
    private readonly ActivityTracker _activityTracker;
    private readonly ILogger<CallbackHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CallbackHandler"/> class.
    /// </summary>
    /// <param name="botClient">The Telegram bot client instance.</param>
    /// <param name="conversionService">The conversion service for routing to converters.</param>
    /// <param name="tempFileService">The temp file service for cleanup.</param>
    /// <param name="localizationService">The localization service for multi-language support.</param>
    /// <param name="adminService">The admin service for admin operations.</param>
    /// <param name="activityTracker">The activity tracker for tracking user activity.</param>
    /// <param name="logger">The logger instance.</param>
    public CallbackHandler(
        TelegramBotClient botClient,
        ConversionService conversionService,
        TempFileService tempFileService,
        LocalizationService localizationService,
        AdminService adminService,
        ActivityTracker activityTracker,
        ILogger<CallbackHandler> logger)
    {
        _botClient = botClient;
        _conversionService = conversionService;
        _tempFileService = tempFileService;
        _localizationService = localizationService;
        _adminService = adminService;
        _activityTracker = activityTracker;
        _logger = logger;
    }

    /// <summary>
    /// Handles a callback query from an inline keyboard button click.
    /// </summary>
    /// <param name="callbackQuery">The callback query object from Telegram.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var callbackData = callbackQuery.Data!;

        _logger.LogInformation("Received callback: {CallbackData} from chat {ChatId}", callbackData, chatId);

        // Always answer callback query first to remove loading spinner
        try
        {
            await _botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to answer callback query");
        }

        // Check if this is an admin callback
        if (KeyboardBuilder.TryParseAdminCallback(callbackData, out var adminAction, out var adminParam))
        {
            if (!_adminService.IsAdmin(chatId))
            {
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetAdminNotAuthorized(chatId),
                    cancellationToken: cancellationToken);
                return;
            }

            await HandleAdminCallbackAsync(chatId, adminAction, adminParam, cancellationToken);
            return;
        }

        // Check if this is a language selection callback
        if (KeyboardBuilder.TryParseLanguageCallback(callbackData, out var newLanguage))
        {
            await HandleLanguageChangeAsync(chatId, newLanguage, cancellationToken);
            return;
        }

        // Check if this is a "select language" button (show language options)
        if (callbackData == "lang:select")
        {
            var keyboard = KeyboardBuilder.BuildLanguageKeyboard(_localizationService, chatId);
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetSelectLanguage(chatId),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
            return;
        }

        // Parse conversion callback data
        if (!KeyboardBuilder.ParseCallbackData(callbackData, out var sourceType, out var targetFormat))
        {
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetInvalidConversion(chatId),
                cancellationToken: cancellationToken);
            return;
        }

        // Lookup active job
        if (!UpdateHandler.ActiveJobs.TryGetValue(chatId, out var job))
        {
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetSessionExpired(chatId),
                cancellationToken: cancellationToken);
            return;
        }

        // Verify the job matches the callback
        if (job.DetectedType != sourceType)
        {
            await _botClient.SendMessage(
                chatId,
                _localizationService.GetConversionMismatch(chatId),
                cancellationToken: cancellationToken);
            UpdateHandler.ActiveJobs.TryRemove(chatId, out _);
            _tempFileService.CleanupJob(chatId);
            return;
        }

        // Send conversion in progress message
        var progressMessage = await _botClient.SendMessage(
            chatId,
            _localizationService.GetConverting(chatId),
            cancellationToken: cancellationToken);

        try
        {
            // Perform conversion
            var outputPath = await _conversionService.ConvertAsync(job, targetFormat, cancellationToken);

            // Verify output file exists
            if (!System.IO.File.Exists(outputPath))
            {
                throw new FileNotFoundException("Converted file was not created", outputPath);
            }

            // Send converted file back to user
            await using var fileStream = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = Path.GetFileName(outputPath);
            var inputFile = new InputFileStream(fileStream, fileName);

            await _botClient.SendDocument(
                chatId: new Telegram.Bot.Types.ChatId(chatId),
                document: inputFile,
                caption: _localizationService.GetDone(chatId),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Conversion successful for chat {ChatId}: {Source} → {Target}",
                chatId, sourceType, targetFormat);

            // Track conversion
            _activityTracker.RecordConversion(chatId, sourceType.ToString());
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Conversion cancelled for chat {ChatId}", chatId);
            await _botClient.EditMessageText(
                chatId,
                progressMessage.MessageId,
                _localizationService.GetConversionCancelled(chatId),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion failed for chat {ChatId}: {Source} → {Target}", chatId, sourceType, targetFormat);
            await _botClient.EditMessageText(
                chatId,
                progressMessage.MessageId,
                _localizationService.GetConversionFailed(chatId),
                cancellationToken: cancellationToken);
        }
        finally
        {
            // Clean up temp files and remove job
            UpdateHandler.ActiveJobs.TryRemove(chatId, out _);
            _tempFileService.CleanupJob(chatId);
        }
    }

    /// <summary>
    /// Handles language change callback.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="language">The new language to set.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private async Task HandleLanguageChangeAsync(long chatId, UserLanguage language, CancellationToken cancellationToken)
    {
        _localizationService.SetLanguage(chatId, language);

        // Send confirmation in the NEW language
        await _botClient.SendMessage(
            chatId,
            _localizationService.GetLanguageChanged(chatId),
            cancellationToken: cancellationToken);

        // If there's an active job, resend the conversion options in the new language
        if (UpdateHandler.ActiveJobs.TryGetValue(chatId, out var job))
        {
            var emoji = job.DetectedType switch
            {
                Models.FileType.Docx => "📄",
                Models.FileType.Pdf => "📕",
                Models.FileType.Pptx => "📽",
                Models.FileType.Xlsx => "📊",
                Models.FileType.Image => "🖼",
                _ => "📎"
            };

            var keyboard = KeyboardBuilder.BuildConversionKeyboard(
                job.DetectedType,
                _localizationService,
                chatId);

            await _botClient.SendMessage(
                chatId,
                _localizationService.GetDetectedFile(chatId, emoji, job.DetectedType.ToString()),
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Handles admin panel callbacks.
    /// </summary>
    private async Task HandleAdminCallbackAsync(long chatId, string action, string? parameter, CancellationToken cancellationToken)
    {
        switch (action)
        {
            case "stats":
                await _botClient.SendMessage(
                    chatId,
                    _adminService.GetStatsMessage(),
                    cancellationToken: cancellationToken);
                break;

            case "users":
                await HandleAdminUsersAsync(chatId, parameter, cancellationToken);
                break;

            case "broadcast":
                UpdateHandler.BroadcastWaiting[chatId] = true;
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetAdminBroadcastPrompt(chatId) + "\n\nSend /cancel to abort.",
                    cancellationToken: cancellationToken);
                break;

            case "reset":
                await _botClient.SendMessage(
                    chatId,
                    _adminService.ResetStats(),
                    cancellationToken: cancellationToken);
                break;

            case "back":
                var keyboard = KeyboardBuilder.BuildAdminKeyboard(_localizationService, chatId);
                await _botClient.SendMessage(
                    chatId,
                    _localizationService.GetAdminWelcome(chatId),
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
                break;

            default:
                await _botClient.SendMessage(
                    chatId,
                    "⚠️ Unknown admin action.",
                    cancellationToken: cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Handles admin users list callback.
    /// </summary>
    private async Task HandleAdminUsersAsync(long chatId, string? parameter, CancellationToken cancellationToken)
    {
        int page = 1;

        if (!string.IsNullOrEmpty(parameter) && parameter.StartsWith("page:"))
        {
            var pageStr = parameter.Substring(5);
            if (int.TryParse(pageStr, out var parsedPage))
            {
                page = parsedPage;
            }
        }

        var users = _activityTracker.GetAllUsers();
        var pageSize = 10;
        var totalPages = (int)Math.Ceiling((double)users.Count / pageSize);
        if (totalPages == 0) totalPages = 1;
        page = Math.Max(1, Math.Min(page, totalPages));

        var message = _adminService.GetUserListMessage(page, pageSize);
        var keyboard = KeyboardBuilder.BuildUserPaginationKeyboard(page, totalPages);

        await _botClient.SendMessage(
            chatId,
            message,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
