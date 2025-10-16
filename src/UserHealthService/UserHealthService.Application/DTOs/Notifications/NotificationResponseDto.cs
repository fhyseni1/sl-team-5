using System;

namespace UserHealthService.Domain.DTOs.Notifications
{
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ActionUrl { get; set; }
        public string? Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TypeDisplay => Type.ToString();
        public bool IsScheduled => ScheduledTime > DateTime.UtcNow;
        public bool IsOverdue => !IsRead && ScheduledTime < DateTime.UtcNow.AddHours(-1);
    }
}