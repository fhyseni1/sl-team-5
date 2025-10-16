using MedicationService.Domain.Entities;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationScheduleRepository : IRepository<MedicationSchedule>
    {
        Task<IEnumerable<MedicationSchedule>> GetByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<MedicationSchedule>> GetActiveSchedulesAsync();
        Task<IEnumerable<MedicationSchedule>> GetSchedulesByFrequencyAsync(Domain.Enums.FrequencyType frequency);
        Task<IEnumerable<MedicationSchedule>> GetUpcomingSchedulesAsync(DateTime startTime, DateTime endTime);
    }
}

