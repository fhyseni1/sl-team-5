using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Interfaces
{
    public interface IDrugInteractionRepository : IRepository<DrugInteraction>
    {
        Task<IEnumerable<DrugInteraction>> GetByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<DrugInteraction>> GetBySeverityAsync(InteractionSeverity severity);
        Task<IEnumerable<DrugInteraction>> GetUnacknowledgedAsync();
        Task<DrugInteraction?> GetByMedicationAndDrugAsync(Guid medicationId, string drugName);
    }
}

