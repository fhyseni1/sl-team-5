using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Interfaces
{
    public interface ISymptomLogRepository
    {
        Task<SymptomLog?> GetByIdAsync(Guid id);
        Task<IEnumerable<SymptomLog>> GetAllAsync();
        Task<IEnumerable<SymptomLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<SymptomLog>> GetByUserIdAndSeverityAsync(Guid userId, SymptomSeverity severity);
        Task<SymptomLog> AddAsync(SymptomLog symptomLog);
        Task<SymptomLog> UpdateAsync(SymptomLog symptomLog);
        Task<bool> DeleteAsync(Guid id);
    }
}