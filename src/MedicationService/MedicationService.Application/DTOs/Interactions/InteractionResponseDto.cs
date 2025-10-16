using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Domain.DTOs.Interactions
{
    public class InteractionResponseDto
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string InteractingDrugName { get; set; } = string.Empty;
        public string InteractionDescription { get; set; } = string.Empty;
        public InteractionSeverity Severity { get; set; }
        public string ClinicalEffect { get; set; } = string.Empty;
        public string ManagementRecommendation { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public bool IsAcknowledged { get; set; }
    }
}