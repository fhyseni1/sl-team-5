using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IAllergyRepository
    {
        Task<Allergy?> GetByIdAsync(Guid id); 
        Task<IEnumerable<Allergy>> GetAllAsync();
        Task<IEnumerable<Allergy>> GetByUserIdAsync(Guid userId);
        Task<Allergy> AddAsync(Allergy allergy);
        Task<Allergy> UpdateAsync(Allergy allergy);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}