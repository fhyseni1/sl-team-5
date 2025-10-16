using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Domain.DTOs.Reminders
{
    public class ReminderUpdateDto
    {
        public DateTime ScheduledTime { get; set; }
        public string? Message { get; set; }
        public string? NotificationChannel { get; set; }
    }
}