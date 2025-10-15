namespace MedicationService.Domain.Entities
{
    public class Prescription
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public Guid PrescribingDoctorId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public DateTime PrescribedDate { get; set; }
        public int RefillsRemaining { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? PrescriptionImageUrl { get; set; }
        public string? ScannedText { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public virtual Medication Medication { get; set; } = null!;
    }
}

