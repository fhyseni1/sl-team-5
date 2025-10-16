using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Doses
{
    public class DoseResponseDto
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public DateTime? TakenTime { get; set; }
        public bool IsTaken { get; set; }
        public bool IsMissed { get; set; }
        public string? Notes { get; set; }
        public decimal? ActualDosage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}