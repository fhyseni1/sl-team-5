namespace MedicationService.Domain.Entities
{
    public class MedicationDose
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? TakenTime { get; set; }
        public bool IsTaken { get; set; }
        public bool IsMissed { get; set; }
        public string? Notes { get; set; }
        public decimal? ActualDosage { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public virtual Medication Medication { get; set; } = null!;
    }
}

