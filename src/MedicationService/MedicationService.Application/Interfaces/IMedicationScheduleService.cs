using MedicationService.Application.DTOs.Schedules;
using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationScheduleService
    {
        Task<IEnumerable<ScheduleResponseDto>> GetAllAsync();

        Task<IEnumerable<ScheduleResponseDto>> GetActiveSchedulesAsync();

        Task<IEnumerable<ScheduleResponseDto>> GetByMedicationIdAsync(Guid medicationId);

        Task<IEnumerable<ScheduleResponseDto>> GetByFrequencyAsync(FrequencyType frequency);

        Task<ScheduleResponseDto?> GetByIdAsync(Guid id);

        Task<IEnumerable<ScheduleResponseDto>> GetUpcomingSchedulesAsync(DateTime startTime, DateTime endTime);

        Task<bool> DeleteAsync(Guid id);
        Task<ScheduleResponseDto> CreateAsync(ScheduleCreateDto dto);
        Task<ScheduleResponseDto?> UpdateAsync(Guid id, ScheduleUpdateDto dto);
        Task<bool> ExistsAsync(Guid id);

    }
}
