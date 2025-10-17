using System;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.DTOs.Notifications
{
    public class NotificationCreateDto
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public string? ActionUrl { get; set; }
        public string? Priority { get; set; } = "Normal";
    }
}