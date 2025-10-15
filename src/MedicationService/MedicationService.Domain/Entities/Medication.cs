using MedicationService.Domain.Enums;

namespace MedicationService.Domain.Entities
{
    public class Medication
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public MedicationType Type { get; set; }
        public decimal Dosage { get; set; }
        public DosageUnit DosageUnit { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public MedicationStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public string? Barcode { get; set; }
        public string? QRCode { get; set; }
        public string? NDCCode { get; set; }
        public ScanningMethod ScanningMethod { get; set; }
        public string? ScannedImageUrl { get; set; }
        
        public virtual ICollection<MedicationSchedule> Schedules { get; set; } = new List<MedicationSchedule>();
        public virtual ICollection<MedicationDose> Doses { get; set; } = new List<MedicationDose>();
        public virtual ICollection<DrugInteraction> DrugInteractions { get; set; } = new List<DrugInteraction>();
        public virtual Prescription? Prescription { get; set; }
    }
}

