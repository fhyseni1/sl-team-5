using MedicationService.Domain.Entities;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationRepository : IRepository<Medication>
    {
        Task<IEnumerable<Medication>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Medication>> GetActiveByUserIdAsync(Guid userId);
        Task<Medication?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Medication>> GetByStatusAsync(Domain.Enums.MedicationStatus status);
        Task<IEnumerable<Medication>> SearchByNameAsync(string name);
   }
}

