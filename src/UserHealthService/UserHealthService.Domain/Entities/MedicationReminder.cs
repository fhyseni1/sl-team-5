using System;

namespace UserHealthService.Domain.Entities
{
    public class MedicationReminder
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid MedicationId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public ReminderStatus Status { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Medication Medication { get; set; } = null!;
    }

    public enum ReminderStatus
    {
        Scheduled = 1,
        Sent = 2,
        Failed = 3,
        Cancelled = 4
    }
}