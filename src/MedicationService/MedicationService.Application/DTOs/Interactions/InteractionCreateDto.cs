using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Interactions
{
    public class InteractionCreateDto
    {
        public Guid MedicationId { get; set; }
        public string InteractingDrugName { get; set; } = string.Empty;
        public string InteractionDescription { get; set; } = string.Empty;
        public InteractionSeverity Severity { get; set; }
        public string ClinicalEffect { get; set; } = string.Empty;
        public string ManagementRecommendation { get; set; } = string.Empty;
    }
}