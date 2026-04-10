namespace TelegramConverterBot.Models;

/// <summary>
/// Represents statistics for the bot.
/// </summary>
public class AdminStats
{
    /// <summary>
    /// Total number of unique users who have interacted with the bot.
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Total number of conversions performed.
    /// </summary>
    public int TotalConversions { get; set; }

    /// <summary>
    /// Number of active users in the last 24 hours.
    /// </summary>
    public int ActiveUsersToday { get; set; }

    /// <summary>
    /// Number of active users in the last 7 days.
    /// </summary>
    public int ActiveUsersThisWeek { get; set; }

    /// <summary>
    /// Number of active users in the last 30 days.
    /// </summary>
    public int ActiveUsersThisMonth { get; set; }

    /// <summary>
    /// Number of currently active conversion jobs.
    /// </summary>
    public int ActiveJobs { get; set; }

    /// <summary>
    /// Conversions broken down by source type.
    /// </summary>
    public Dictionary<string, int> ConversionsByType { get; set; } = new();

    /// <summary>
    /// Bot uptime.
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// Bot start time.
    /// </summary>
    public DateTime StartedAt { get; set; }
}

/// <summary>
/// Represents user activity information.
/// </summary>
public class UserActivity
{
    /// <summary>
    /// The Telegram chat ID.
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// The username of the user (if available).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The first name of the user.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// The last name of the user.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// The preferred language of the user.
    /// </summary>
    public UserLanguage Language { get; set; }

    /// <summary>
    /// Number of conversions by this user.
    /// </summary>
    public int ConversionCount { get; set; }

    /// <summary>
    /// The last time the user interacted with the bot.
    /// </summary>
    public DateTime LastActivity { get; set; }

    /// <summary>
    /// The first time the user interacted with the bot.
    /// </summary>
    public DateTime FirstSeen { get; set; }
}
