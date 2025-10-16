using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Reminders
{
    public class ReminderResponseDto
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public Guid? ScheduleId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public ReminderStatus Status { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public int SnoozeCount { get; set; }
        public string? Message { get; set; }
        public string? NotificationChannel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}