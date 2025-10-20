using UserHealthService.Application.DTOs.Symptoms;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Interfaces
{
    public interface ISymptomLogService
    {
        Task<SymptomLogResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<SymptomLogResponseDto>> GetAllAsync();
        Task<IEnumerable<SymptomLogResponseDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<SymptomLogResponseDto>> GetByUserIdAndSeverityAsync(Guid userId, SymptomSeverity severity);
        Task<SymptomLogResponseDto> CreateAsync(SymptomLogCreateDto createDto);
        Task<SymptomLogResponseDto?> UpdateAsync(Guid id, SymptomLogUpdateDto updateDto);
        Task<bool> DeleteAsync(Guid id);
    }
}