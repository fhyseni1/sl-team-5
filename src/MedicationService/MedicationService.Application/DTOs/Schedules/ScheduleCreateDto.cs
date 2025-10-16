using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Schedules
{
    public class ScheduleCreateDto
    {
        public Guid MedicationId { get; set; }
        public FrequencyType Frequency { get; set; }
        public int CustomFrequencyHours { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public string DaysOfWeek { get; set; } = string.Empty;
    }
}