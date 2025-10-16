using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Domain.DTOs.Doses
{
    public class DoseCreateDto
    {
        public Guid MedicationId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string? Notes { get; set; }
        public decimal? ActualDosage { get; set; }
    }
}