using MedicationService.Application.DTOs.Schedules;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Services
{
    public class ScheduleGeneratorService : IScheduleGeneratorService
    {
        private const string AllDaysOfWeek = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday";

        public List<ScheduleCreateDto> GenerateSchedules(
            Guid medicationId,
            FrequencyType frequency,
            int? customFrequencyHours,
            string? daysOfWeek,
            int? monthlyDay)
        {
            var schedules = new List<ScheduleCreateDto>();

            switch (frequency)
            {
                case FrequencyType.OnceDaily:
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(9, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    break;

                case FrequencyType.TwiceDaily:
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(9, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(21, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    break;

                case FrequencyType.ThreeTimesDaily:
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(8, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(14, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(20, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    break;

                case FrequencyType.FourTimesDaily:
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(6, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(12, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(18, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(22, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    break;

                case FrequencyType.EveryFewHours:
                    var hoursForEveryFew = customFrequencyHours ?? 24;
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(9, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = hoursForEveryFew
                    });
                    break;

                case FrequencyType.AsNeeded:
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(9, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = 0
                    });
                    break;

                case FrequencyType.Weekly:
                    var weeklyDay = string.IsNullOrWhiteSpace(daysOfWeek) ? "Monday" : daysOfWeek;
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(9, 0, 0),
                        DaysOfWeek = weeklyDay,
                        CustomFrequencyHours = 0
                    });
                    break;

                case FrequencyType.Monthly:
                    var dayOfMonth = monthlyDay?.ToString() ?? "1";
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(9, 0, 0),
                        DaysOfWeek = dayOfMonth,
                        CustomFrequencyHours = 0
                    });
                    break;

                case FrequencyType.Custom:
                    var customHours = customFrequencyHours ?? 24;
                    schedules.Add(new ScheduleCreateDto
                    {
                        MedicationId = medicationId,
                        Frequency = frequency,
                        TimeOfDay = new TimeSpan(9, 0, 0),
                        DaysOfWeek = AllDaysOfWeek,
                        CustomFrequencyHours = customHours
                    });
                    break;
            }

            return schedules;
        }
    }
}

