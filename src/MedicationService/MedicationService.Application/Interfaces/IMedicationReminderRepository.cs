using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationReminderRepository : IRepository<MedicationReminder>
    {
        Task<IEnumerable<MedicationReminder>> GetByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<MedicationReminder>> GetPendingRemindersAsync();
        Task<IEnumerable<MedicationReminder>> GetByStatusAsync(ReminderStatus status);
        Task<IEnumerable<MedicationReminder>> GetUpcomingRemindersAsync(DateTime beforeTime);
        Task<IEnumerable<MedicationReminder>> GetMissedRemindersAsync();
    }
}

