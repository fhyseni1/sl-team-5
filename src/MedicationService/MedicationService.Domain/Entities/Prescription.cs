using MedicationService.Domain.Enums;

namespace MedicationService.Domain.Entities
{
    public class Prescription
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string PrescriberName { get; set; } = string.Empty;
        public string PrescriberContact { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyContact { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public PrescriptionStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int RemainingRefills { get; set; }

        public virtual Medication Medication { get; set; } = null!;
    }
}