using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Domain.DTOs.Schedules
{
    public class ScheduleResponseDto
    {
        public Guid Id { get; set; }
        public Guid MedicationId { get; set; }
        public FrequencyType Frequency { get; set; }
        public int CustomFrequencyHours { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string MedicationName { get; set; } = string.Empty;
    }
}