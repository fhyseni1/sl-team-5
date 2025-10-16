using MedicationService.Application.DTOs.Doses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicationService.Application.Interfaces
{
    public interface IMedicationDoseService
    {
        Task<IEnumerable<DoseResponseDto>> GetAllAsync();
        Task<DoseResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<DoseResponseDto>> GetByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<DoseResponseDto>> GetTodayDosesAsync(Guid userId);
        Task<IEnumerable<DoseResponseDto>> GetMissedDosesAsync(Guid userId);
        Task<DoseResponseDto> CreateAsync(DoseCreateDto dto);
        Task<DoseResponseDto?> UpdateAsync(Guid id, DoseUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
