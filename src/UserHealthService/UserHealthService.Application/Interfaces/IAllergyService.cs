using UserHealthService.Application.DTOs.Allergies;

namespace UserHealthService.Application.Interfaces
{
    public interface IAllergyService
    {
        Task<AllergyResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<AllergyResponseDto>> GetAllAsync();
        Task<IEnumerable<AllergyResponseDto>> GetByUserIdAsync(Guid userId);
        Task<int> GetAllergyCountAsync(Guid userId);
        Task<AllergyResponseDto> CreateAsync(AllergyCreateDto createDto);
        Task<AllergyResponseDto?> UpdateAsync(Guid id, AllergyUpdateDto updateDto); 
        Task<bool> DeleteAsync(Guid id);
        Task<object> CheckAllergyConflictsAsync(Guid userId, string medicationName);
    }
}