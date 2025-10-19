using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationReminderService
    {
        Task<ReminderResponseDto> CreateAsync(ReminderCreateDto dto);
        Task<ReminderResponseDto?> UpdateAsync(Guid id, ReminderUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<ReminderResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ReminderResponseDto>> GetAllAsync();
        Task<IEnumerable<ReminderResponseDto>> GetByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<ReminderResponseDto>> GetByStatusAsync(ReminderStatus status);
        Task<IEnumerable<ReminderResponseDto>> GetMissedRemindersAsync();
        Task<IEnumerable<ReminderResponseDto>> GetPendingRemindersAsync();
        Task<IEnumerable<ReminderResponseDto>> GetUpcomingRemindersAsync(DateTime beforeTime);
        Task<int> GetTotalCountAsync();
        Task<bool> ExistsAsync(Guid id);
    }
}
