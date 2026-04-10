using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TelegramConverterBot.Models;

namespace TelegramConverterBot.Services;

/// <summary>
/// Tracks user activity and bot statistics for admin panel.
/// </summary>
public class ActivityTracker
{
    private readonly ILogger<ActivityTracker> _logger;
    private readonly DateTime _startTime;

    /// <summary>
    /// Stores user activity information, keyed by chatId.
    /// </summary>
    private static readonly ConcurrentDictionary<long, UserActivity> _userActivities = new();

    /// <summary>
    /// Stores conversion counts by file type.
    /// </summary>
    private static readonly ConcurrentDictionary<string, int> _conversionsByType = new();

    /// <summary>
    /// Total conversion count.
    /// </summary>
    private static int _totalConversions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityTracker"/> class.
    /// </summary>
    public ActivityTracker(ILogger<ActivityTracker> logger)
    {
        _logger = logger;
        _startTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a new user interaction.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="username">The username (if available).</param>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name (if available).</param>
    /// <param name="language">The user's language.</param>
    public void RecordUserActivity(long chatId, string? username, string? firstName, string? lastName, UserLanguage language)
    {
        var activity = _userActivities.AddOrUpdate(
            chatId,
            id => new UserActivity
            {
                ChatId = id,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Language = language,
                ConversionCount = 0,
                FirstSeen = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            },
            (id, existing) =>
            {
                existing.LastActivity = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(username))
                    existing.Username = username;
                if (!string.IsNullOrWhiteSpace(firstName))
                    existing.FirstName = firstName;
                if (!string.IsNullOrWhiteSpace(lastName))
                    existing.LastName = lastName;
                existing.Language = language;
                return existing;
            });

        _logger.LogDebug("Recorded activity for user {ChatId}", chatId);
    }

    /// <summary>
    /// Records a successful conversion.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="fileType">The file type that was converted.</param>
    public void RecordConversion(long chatId, string fileType)
    {
        Interlocked.Increment(ref _totalConversions);

        _conversionsByType.AddOrUpdate(
            fileType,
            1,
            (key, existing) => existing + 1);

        if (_userActivities.TryGetValue(chatId, out var activity))
        {
            activity.ConversionCount++;
        }

        _logger.LogDebug("Recorded conversion for user {ChatId}, type {FileType}", chatId, fileType);
    }

    /// <summary>
    /// Gets the bot statistics for admin panel.
    /// </summary>
    public AdminStats GetStats(int activeJobs = 0)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddDays(-30);

        return new AdminStats
        {
            TotalUsers = _userActivities.Count,
            TotalConversions = _totalConversions,
            ActiveUsersToday = _userActivities.Count(x => x.Value.LastActivity >= today),
            ActiveUsersThisWeek = _userActivities.Count(x => x.Value.LastActivity >= weekAgo),
            ActiveUsersThisMonth = _userActivities.Count(x => x.Value.LastActivity >= monthAgo),
            ActiveJobs = activeJobs,
            ConversionsByType = _conversionsByType.ToDictionary(x => x.Key, x => x.Value),
            Uptime = now - _startTime,
            StartedAt = _startTime
        };
    }

    /// <summary>
    /// Gets a list of all users with their activity.
    /// </summary>
    public List<UserActivity> GetAllUsers()
    {
        return _userActivities.Values
            .OrderByDescending(x => x.LastActivity)
            .ToList();
    }

    /// <summary>
    /// Gets a specific user's activity.
    /// </summary>
    public UserActivity? GetUserActivity(long chatId)
    {
        return _userActivities.GetValueOrDefault(chatId);
    }

    /// <summary>
    /// Gets the list of all active chat IDs.
    /// </summary>
    public List<long> GetAllChatIds()
    {
        return _userActivities.Keys.ToList();
    }

    /// <summary>
    /// Resets the conversion statistics (keeps user data).
    /// </summary>
    public void ResetStats()
    {
        _totalConversions = 0;
        _conversionsByType.Clear();

        foreach (var activity in _userActivities.Values)
        {
            activity.ConversionCount = 0;
        }

        _logger.LogInformation("Statistics reset by admin");
    }
}
