using MedicationService.Domain.Entities;

namespace MedicationService.Application.Interfaces
{
    public interface IPrescriptionRepository : IRepository<Prescription>
    {
        Task<Prescription?> GetByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<Prescription>> GetExpiringPrescriptionsAsync(DateTime beforeDate);
        Task<IEnumerable<Prescription>> GetLowRefillPrescriptionsAsync(int maxRefills);
        Task<Prescription?> GetByPrescriptionNumberAsync(string prescriptionNumber);
    }
}

