using MedicationService.Domain.Enums;

namespace MedicationService.Domain.Entities
{
    public class MedicationSchedule
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public FrequencyType Frequency { get; set; }
        public int CustomFrequencyHours { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual Medication Medication { get; set; } = null!;
        public virtual ICollection<MedicationReminder> Reminders { get; set; } = new List<MedicationReminder>();
    }
}

