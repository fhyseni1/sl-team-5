using MedicationService.Domain.DTOs.Medications;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationService
    {
        Task<MedicationResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<MedicationResponseDto>> GetAllAsync();
        Task<IEnumerable<MedicationResponseDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<MedicationResponseDto>> GetActiveByUserIdAsync(Guid userId);
        Task<MedicationResponseDto?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<MedicationResponseDto>> SearchByNameAsync(string name);
        Task<MedicationResponseDto> CreateAsync(MedicationCreateDto createDto);
        Task<MedicationResponseDto?> UpdateAsync(Guid id, MedicationUpdateDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}

