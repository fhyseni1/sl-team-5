using MedicationService.Domain.Enums;

namespace MedicationService.Domain.Entities
{
    public class MedicationReminder
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public Guid? ScheduleId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public ReminderStatus Status { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public int SnoozeCount { get; set; }
        public string? Message { get; set; }
        public string? NotificationChannel { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public virtual Medication Medication { get; set; } = null!;
        public virtual MedicationSchedule? Schedule { get; set; }
    }
}

