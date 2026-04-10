using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramConverterBot.Bot;
using TelegramConverterBot.Models;

namespace TelegramConverterBot.Services;

/// <summary>
/// Handles admin operations including authorization, broadcast, and statistics.
/// </summary>
public class AdminService
{
    private readonly TelegramBotClient _botClient;
    private readonly ActivityTracker _activityTracker;
    private readonly long[] _adminIds;
    private readonly ILogger<AdminService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminService"/> class.
    /// </summary>
    public AdminService(
        TelegramBotClient botClient,
        ActivityTracker activityTracker,
        BotConfiguration config,
        ILogger<AdminService> logger)
    {
        _botClient = botClient;
        _activityTracker = activityTracker;
        _adminIds = config.AdminIds;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a user is an administrator.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>True if the user is an admin.</returns>
    public bool IsAdmin(long chatId)
    {
        return _adminIds.Contains(chatId);
    }

    /// <summary>
    /// Gets formatted statistics message for admin.
    /// </summary>
    public string GetStatsMessage()
    {
        var stats = _activityTracker.GetStats();
        
        var conversionTypeStats = stats.ConversionsByType
            .Select(kv => $"   • {kv.Key}: {kv.Value}")
            .DefaultIfEmpty("   No conversions yet");

        return $@"📊 **Bot Statistics**

👥 **Users:**
   • Total: {stats.TotalUsers}
   • Active (24h): {stats.ActiveUsersToday}
   • Active (7d): {stats.ActiveUsersThisWeek}
   • Active (30d): {stats.ActiveUsersThisMonth}

🔄 **Conversions:**
   • Total: {stats.TotalConversions}
   • Active jobs: {stats.ActiveJobs}

📈 **By Type:**
{string.Join("\n", conversionTypeStats)}

⏱ **Uptime:** {stats.Uptime.Days}d {stats.Uptime.Hours}h {stats.Uptime.Minutes}m
🚀 **Started:** {stats.StartedAt:yyyy-MM-dd HH:mm:ss} UTC";
    }

    /// <summary>
    /// Gets a formatted user list message.
    /// </summary>
    public string GetUserListMessage(int page = 1, int pageSize = 10)
    {
        var users = _activityTracker.GetAllUsers();
        var totalPages = (int)Math.Ceiling((double)users.Count / pageSize);
        if (totalPages == 0) totalPages = 1;

        page = Math.Max(1, Math.Min(page, totalPages));
        var start = (page - 1) * pageSize;
        var pageUsers = users.Skip(start).Take(pageSize).ToList();

        if (pageUsers.Count == 0)
        {
            return "👥 No users yet. Users will appear here when they interact with the bot.";
        }

        var userList = pageUsers.Select((u, i) =>
        {
            var name = u.Username ?? $"{u.FirstName} {u.LastName}".Trim();
            return $"{start + i + 1}. ID: `{u.ChatId}` | {EscapeMarkdown(name)} | 🌐 {u.Language} | 🔄 {u.ConversionCount}";
        });

        return $@"👥 **Users (Page {page}/{totalPages})**

{string.Join("\n", userList)}

💡 Tap a user ID for details.";
    }

    /// <summary>
    /// Gets formatted user details.
    /// </summary>
    public string GetUserDetailsMessage(long chatId)
    {
        var user = _activityTracker.GetUserActivity(chatId);
        if (user == null)
        {
            return "⚠️ User not found.";
        }

        var name = user.Username ?? $"{user.FirstName} {user.LastName}".Trim() ?? "Unknown";

        return $@"👤 **User Details**

🆔 ID: `{user.ChatId}`
👤 Name: {EscapeMarkdown(name)}
🌐 Language: {user.Language}

📊 **Activity:**
   • Conversions: {user.ConversionCount}
   • First seen: {user.FirstSeen:yyyy-MM-dd HH:mm:ss}
   • Last active: {user.LastActivity:yyyy-MM-dd HH:mm:ss}";
    }

    /// <summary>
    /// Broadcasts a message to all active users.
    /// </summary>
    public async Task<BroadcastResult> BroadcastMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        var chatIds = _activityTracker.GetAllChatIds();
        int success = 0;
        int failed = 0;
        var errors = new List<string>();

        foreach (var chatId in chatIds)
        {
            try
            {
                await _botClient.SendMessage(chatId, message, cancellationToken: cancellationToken);
                success++;
                _logger.LogInformation("Broadcast sent to {ChatId}", chatId);
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"{chatId}: {ex.Message}");
                _logger.LogWarning(ex, "Failed to broadcast to {ChatId}", chatId);
            }

            // Add small delay to avoid rate limiting
            await Task.Delay(50, cancellationToken);
        }

        return new BroadcastResult
        {
            Total = chatIds.Count,
            Success = success,
            Failed = failed,
            Errors = errors
        };
    }

    /// <summary>
    /// Gets a formatted broadcast result message.
    /// </summary>
    public string GetBroadcastResultMessage(BroadcastResult result)
    {
        var msg = $@"📢 **Broadcast Complete**

✅ Success: {result.Success}/{result.Total}
❌ Failed: {result.Failed}/{result.Total}";

        if (result.Errors.Any())
        {
            var errorList = result.Errors.Take(5).Select(e => $"   • {e}");
            msg += $"\n\n**Errors:**\n{string.Join("\n", errorList)}";
            if (result.Errors.Count > 5)
            {
                msg += $"\n   ... and {result.Errors.Count - 5} more";
            }
        }

        return msg;
    }

    /// <summary>
    /// Resets bot statistics.
    /// </summary>
    public string ResetStats()
    {
        _activityTracker.ResetStats();
        return "📊 **Statistics Reset**\n\nAll conversion counters have been reset to zero.\nUser activity data is preserved.";
    }

    /// <summary>
    /// Escapes special Markdown characters for Telegram.
    /// </summary>
    private static string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var escapeChars = new[] { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };
        foreach (var ch in escapeChars)
        {
            text = text.Replace(ch.ToString(), $"\\{ch}");
        }
        return text;
    }
}

/// <summary>
/// Result of a broadcast operation.
/// </summary>
public class BroadcastResult
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
