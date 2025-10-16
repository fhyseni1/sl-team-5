using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Domain.DTOs.Prescriptions
{
    public class PrescriptionUpdateDto
    {
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string PrescriberName { get; set; } = string.Empty;
        public string PrescriberContact { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyContact { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public PrescriptionStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}