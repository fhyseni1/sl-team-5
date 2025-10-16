using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Domain.DTOs.Doses
{
    public class DoseUpdateDto
    {
        public DateTime? TakenTime { get; set; }
        public bool IsTaken { get; set; }
        public string? Notes { get; set; }
        public decimal? ActualDosage { get; set; }
    }
}
