using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Domain.DTOs.Prescriptions
{
    public class PrescriptionResponseDto
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string PrescriberName { get; set; } = string.Empty;
        public string PrescriberContact { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;
        public string PharmacyContact { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public PrescriptionStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
        public int DaysUntilExpiry => ExpiryDate.HasValue ? (int)(ExpiryDate.Value - DateTime.UtcNow).TotalDays : -1;
    }
}