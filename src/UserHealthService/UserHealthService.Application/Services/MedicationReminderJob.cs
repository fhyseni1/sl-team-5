using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Logging;
using UserHealthService.Application.DTOs.Notifications;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Services
{
    public class MedicationReminderJob : IMedicationReminderJob
    {
        private const string MedicationClientName = "MedicationService";
        private const int ScheduledStatusValue = 1; // Matches ReminderStatus.Scheduled in MedicationService
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<MedicationReminderJob> _logger;

        public MedicationReminderJob(
            IHttpClientFactory httpClientFactory,
            INotificationService notificationService,
            INotificationRepository notificationRepository,
            ILogger<MedicationReminderJob> logger)
        {
            _httpClientFactory = httpClientFactory;
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task ProcessReminders()
        {
            var client = _httpClientFactory.CreateClient(MedicationClientName);
            List<MedicationReminderDto>? reminders = null;

            try
            {
                using var response = await client.GetAsync("api/reminders/upcoming");

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    _logger.LogDebug("MedicationService returned 204 No Content for upcoming reminders.");
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("MedicationService returned unexpected status {StatusCode} when fetching reminders.", response.StatusCode);
                    return;
                }

                if (response.Content.Headers.ContentLength == 0)
                {
                    _logger.LogDebug("MedicationService response contained no content.");
                    return;
                }

                var stream = await response.Content.ReadAsStreamAsync();
                if (stream == null || stream.Length == 0)
                {
                    _logger.LogDebug("MedicationService response stream was empty.");
                    return;
                }

                reminders = await JsonSerializer.DeserializeAsync<List<MedicationReminderDto>>(stream, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch upcoming medication reminders from MedicationService.");
                return;
            }

            if (reminders == null || reminders.Count == 0)
            {
                _logger.LogDebug("No medication reminders returned from MedicationService.");
                return;
            }

            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-5);
            var windowEnd = now.AddMinutes(5);

            var dueReminders = reminders
                .Where(r =>
                    r.Status == ScheduledStatusValue &&
                    r.ScheduledTime >= windowStart &&
                    r.ScheduledTime <= windowEnd)
                .ToList();

            if (dueReminders.Count == 0)
            {
                _logger.LogDebug("No medication reminders due within the processing window ({WindowStart} - {WindowEnd}).", windowStart, windowEnd);
                return;
            }

            _logger.LogInformation("Processing {Count} medication reminders due between {WindowStart} and {WindowEnd}.", dueReminders.Count, windowStart, windowEnd);

            var medicationCache = new Dictionary<Guid, MedicationSummaryDto>();
            var notificationCache = new Dictionary<Guid, HashSet<string>>();

            foreach (var reminder in dueReminders.OrderBy(r => r.ScheduledTime))
            {
                if (!medicationCache.TryGetValue(reminder.MedicationId, out var medication))
                {
                    medication = await FetchMedicationAsync(client, reminder.MedicationId);
                    if (medication == null)
                    {
                        _logger.LogWarning("Skipping reminder {ReminderId} because medication {MedicationId} could not be retrieved.", reminder.Id, reminder.MedicationId);
                        continue;
                    }

                    medicationCache[reminder.MedicationId] = medication;
                }

                var userId = medication.UserId;
                if (!notificationCache.TryGetValue(userId, out var existingActionUrls))
                {
                    existingActionUrls = await LoadExistingActionUrlsAsync(userId);
                    notificationCache[userId] = existingActionUrls;
                }

                var actionUrl = $"medication-reminder:{reminder.Id}";
                if (existingActionUrls.Contains(actionUrl))
                {
                    _logger.LogDebug("Notification for reminder {ReminderId} already exists. Skipping.", reminder.Id);
                    continue;
                }

                var priority = string.Equals(reminder.NotificationChannel, "urgent", StringComparison.OrdinalIgnoreCase)
                    ? "Urgent"
                    : "Normal";

                var message = !string.IsNullOrWhiteSpace(reminder.Message)
                    ? reminder.Message
                    : $"It is time to take {reminder.MedicationName}. Scheduled for {reminder.ScheduledTime.ToLocalTime():t}.";

                var titleMedication = string.IsNullOrWhiteSpace(reminder.MedicationName)
                    ? "Medication Reminder"
                    : $"Medication Reminder • {reminder.MedicationName}";

                var notificationDto = new NotificationCreateDto
                {
                    UserId = userId,
                    Type = NotificationType.MedicationReminder,
                    Title = titleMedication,
                    Message = message,
                    ScheduledTime = reminder.ScheduledTime,
                    ActionUrl = actionUrl,
                    Priority = priority
                };

                try
                {
                    await _notificationService.CreateAsync(notificationDto);
                    existingActionUrls.Add(actionUrl);
                    _logger.LogInformation("Created notification for reminder {ReminderId} (user {UserId}).", reminder.Id, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create notification for reminder {ReminderId}.", reminder.Id);
                }
            }
        }

        private async Task<HashSet<string>> LoadExistingActionUrlsAsync(Guid userId)
        {
            try
            {
                var existingNotifications = await _notificationRepository.GetByUserIdAsync(userId);
                return existingNotifications
                    .Where(n => !string.IsNullOrWhiteSpace(n.ActionUrl))
                    .Select(n => n.ActionUrl!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load existing notifications for user {UserId}.", userId);
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private async Task<MedicationSummaryDto?> FetchMedicationAsync(HttpClient client, Guid medicationId)
        {
            try
            {
                return await client.GetFromJsonAsync<MedicationSummaryDto>(
                    $"api/medications/{medicationId}",
                    JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch medication {MedicationId} from MedicationService.", medicationId);
                return null;
            }
        }

        private sealed class MedicationReminderDto
        {
            public Guid Id { get; set; }
            public Guid MedicationId { get; set; }
            public Guid? ScheduleId { get; set; }
            public string? MedicationName { get; set; }
            public DateTime ScheduledTime { get; set; }
            public int Status { get; set; }
            public string? Message { get; set; }
            public string? NotificationChannel { get; set; }
        }

        private sealed class MedicationSummaryDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string? Name { get; set; }
        }
    }
}