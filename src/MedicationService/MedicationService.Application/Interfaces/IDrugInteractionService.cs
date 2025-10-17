using MedicationService.Domain.DTOs.Interactions;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Interfaces
{
    public interface IDrugInteractionService
    {
        Task<IEnumerable<InteractionResponseDto>> GetAllInteractionsAsync();
        Task<InteractionResponseDto?> GetInteractionByIdAsync(Guid id);
        Task<IEnumerable<InteractionResponseDto>> GetInteractionsByMedicationIdAsync(Guid medicationId);
        Task<IEnumerable<InteractionResponseDto>> GetInteractionsBySeverityAsync(InteractionSeverity severity);
        Task<IEnumerable<InteractionResponseDto>> CheckInteractionsAsync(List<Guid> medicationIds);
        Task<InteractionResponseDto> CreateInteractionAsync(InteractionCreateDto createDto);
        Task<InteractionResponseDto?> UpdateInteractionAsync(Guid id, InteractionUpdateDto updateDto);
        Task<bool> DeleteInteractionAsync(Guid id);
    }
}

