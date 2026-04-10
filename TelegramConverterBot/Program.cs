using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;
using TelegramConverterBot.Bot;
using TelegramConverterBot.Converters;
using TelegramConverterBot.Helpers;
using TelegramConverterBot.Services;

namespace TelegramConverterBot;

/// <summary>
/// Application entry point. Configures DI, logging, and starts the bot hosted service.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point. Builds and runs the host.
    /// </summary>
    public static async Task Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/bot-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("🚀 Starting Telegram Converter Bot...");

            var host = CreateHostBuilder(args).Build();

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "💥 Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Creates the host builder with all configured services.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A configured IHostBuilder instance.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(context.HostingEnvironment.ContentRootPath);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();

                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>(optional: true);
                }
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Bind configuration
                var botConfig = new BotConfiguration();
                configuration.Bind(botConfig);
                services.Configure<BotConfiguration>(configuration);

                // Validate bot token
                if (string.IsNullOrWhiteSpace(botConfig.BotToken) ||
                    botConfig.BotToken == "YOUR_TELEGRAM_BOT_TOKEN_HERE")
                {
                    throw new InvalidOperationException(
                        "Bot token is not configured. Set the BotToken in appsettings.json or via environment variables.");
                }

                // Register Telegram Bot Client (both interface and concrete type)
                var botClient = new TelegramBotClient(botConfig.BotToken);
                services.AddSingleton<TelegramBotClient>(botClient);
                services.AddSingleton<ITelegramBotClient>(botClient);

                // Register Helpers
                services.AddSingleton<FileHelper>();

                // Register Services
                services.AddSingleton<LocalizationService>();
                services.AddSingleton<FileDetectorService>();
                services.AddSingleton<TempFileService>(sp =>
                    new TempFileService(botConfig.TempFilesPath, sp.GetRequiredService<ILogger<TempFileService>>()));

                // Register Converters
                services.AddSingleton<DocxToPdfConverter>();
                services.AddSingleton<PdfToDocxConverter>();
                services.AddSingleton<PptxToPdfConverter>();
                services.AddSingleton<XlsxToPdfConverter>();
                services.AddSingleton<ImageToPdfConverter>();

                // Register ConversionService (depends on all converters)
                services.AddSingleton<ConversionService>();

                // Register Bot components
                services.AddSingleton<CallbackHandler>();
                services.AddSingleton<UpdateHandler>();

                // Register Hosted Service
                services.AddHostedService<BotService>();
            });
}
