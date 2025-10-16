using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Reminders
{
    public class ReminderCreateDto
    {
        public Guid MedicationId { get; set; }
        public Guid? ScheduleId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string? Message { get; set; }
        public string? NotificationChannel { get; set; }
    }
}