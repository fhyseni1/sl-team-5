using MedicationService.Domain.Entities;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationDoseRepository : IRepository<MedicationDose>
    {
        Task<IEnumerable<MedicationDose>> GetByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<MedicationDose>> GetMissedDosesAsync(Guid userId);
        Task<IEnumerable<MedicationDose>> GetDosesByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<MedicationDose>> GetTodaysDosesAsync(Guid userId);
        Task<int> GetMissedDoseCountAsync(Guid medicationId);
    }
}

