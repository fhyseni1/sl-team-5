using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Medications
{
    public class MedicationUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public decimal Dosage { get; set; }
        public DosageUnit DosageUnit { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public MedicationStatus Status { get; set; }
        public DateTime? EndDate { get; set; }
    }
}