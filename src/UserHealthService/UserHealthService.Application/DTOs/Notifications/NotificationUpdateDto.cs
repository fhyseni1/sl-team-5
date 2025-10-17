using System;
using UserHealthService.Domain.Enums;
namespace UserHealthService.Application.DTOs.Notifications
{
    public class NotificationUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string? ActionUrl { get; set; }
        public string? Priority { get; set; }
    }
}