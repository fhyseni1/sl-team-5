using MedicationService.Domain.Enums;

namespace MedicationService.Domain.Entities
{
    public class DrugInteraction
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public string InteractingDrugName { get; set; } = string.Empty;
        public string InteractionDescription { get; set; } = string.Empty;
        public InteractionSeverity Severity { get; set; }
        public string ClinicalEffect { get; set; } = string.Empty;
        public string ManagementRecommendation { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public bool IsAcknowledged { get; set; }
        
        public virtual Medication Medication { get; set; } = null!;
    }
}

