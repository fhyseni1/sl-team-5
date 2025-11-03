using MedicationService.Application.DTOs.Schedules;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Interfaces
{
    public interface IScheduleGeneratorService
    {
        List<ScheduleCreateDto> GenerateSchedules(
            Guid medicationId,
            FrequencyType frequency,
            int? customFrequencyHours,
            string? daysOfWeek,
            int? monthlyDay);
    }
}

