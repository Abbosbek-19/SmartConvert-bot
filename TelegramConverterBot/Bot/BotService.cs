using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramConverterBot.Bot;

namespace TelegramConverterBot.Bot;

/// <summary>
/// Hosted service that runs the Telegram bot using long polling.
/// </summary>
public class BotService : IHostedService
{
    private readonly TelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private readonly ILogger<BotService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="BotService"/> class.
    /// </summary>
    /// <param name="botClient">The Telegram bot client instance.</param>
    /// <param name="updateHandler">The update handler for processing messages.</param>
    /// <param name="logger">The logger instance.</param>
    public BotService(
        TelegramBotClient botClient,
        UpdateHandler updateHandler,
        ILogger<BotService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    /// <summary>
    /// Starts the bot by beginning to receive updates via long polling.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Get bot info to verify connection
        var me = await _botClient.GetMe(cancellationToken);
        _logger.LogInformation(
            "🤖 Bot started! Username: @{Username}, Name: {FirstName}",
            me.Username, me.FirstName);

        // Configure receiver options for long polling
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        // Start receiving updates
        _botClient.StartReceiving(
            updateHandler: _updateHandler.HandleUpdateAsync,
            errorHandler: HandlePollingError,
            receiverOptions: receiverOptions,
            cancellationToken: _cancellationTokenSource.Token);

        _logger.LogInformation("Bot is now polling for updates...");
    }

    /// <summary>
    /// Stops the bot by cancelling the polling CancellationToken.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping bot...");

        _cancellationTokenSource?.Cancel();

        // Wait a moment for graceful shutdown
        try
        {
            await Task.Delay(1000, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }

        _logger.LogInformation("Bot stopped successfully");
    }

    /// <summary>
    /// Handles polling errors from the Telegram Bot API.
    /// </summary>
    private Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            Telegram.Bot.Exceptions.ApiRequestException apiEx =>
                $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}",
            _ => $"Unknown Error: {exception.GetType().Name} - {exception.Message}"
        };

        _logger.LogWarning(exception, "Polling error: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }
}
