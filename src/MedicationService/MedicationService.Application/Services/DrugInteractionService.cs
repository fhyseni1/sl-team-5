using AutoMapper;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.DTOs.Interactions;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Services
{
    public class DrugInteractionService : IDrugInteractionService
    {
        private readonly IDrugInteractionRepository _repository;
        private readonly IMedicationRepository _medicationRepository;
        private readonly IMapper _mapper;

        public DrugInteractionService(
            IDrugInteractionRepository repository,
            IMedicationRepository medicationRepository,
            IMapper mapper)
        {
            _repository = repository;
            _medicationRepository = medicationRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InteractionResponseDto>> GetAllInteractionsAsync()
        {
            var interactions = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<InteractionResponseDto>>(interactions);
        }

        public async Task<InteractionResponseDto?> GetInteractionByIdAsync(Guid id)
        {
            var interaction = await _repository.GetByIdAsync(id);
            return interaction == null ? null : _mapper.Map<InteractionResponseDto>(interaction);
        }

        public async Task<IEnumerable<InteractionResponseDto>> GetInteractionsByMedicationIdAsync(Guid medicationId)
        {
            var interactions = await _repository.GetByMedicationIdAsync(medicationId);
            return _mapper.Map<IEnumerable<InteractionResponseDto>>(interactions);
        }

        public async Task<IEnumerable<InteractionResponseDto>> GetInteractionsBySeverityAsync(InteractionSeverity severity)
        {
            var interactions = await _repository.GetBySeverityAsync(severity);
            return _mapper.Map<IEnumerable<InteractionResponseDto>>(interactions);
        }

        public async Task<IEnumerable<InteractionResponseDto>> CheckInteractionsAsync(List<Guid> medicationIds)
        {
            if (medicationIds == null || medicationIds.Count < 2)
                return Enumerable.Empty<InteractionResponseDto>();

            var allInteractions = new List<DrugInteraction>();

            // Check interactions for each medication
            foreach (var medicationId in medicationIds)
            {
                var interactions = await _repository.GetByMedicationIdAsync(medicationId);
                allInteractions.AddRange(interactions);
            }

            // Check if any medications in the list interact with each other
            var medications = new List<Medication>();
            foreach (var id in medicationIds)
            {
                var medication = await _medicationRepository.GetByIdAsync(id);
                if (medication != null)
                {
                    medications.Add(medication);
                }
            }

            // Cross-check medications against each other's interactions
            var crossInteractions = new List<DrugInteraction>();
            foreach (var medication in medications)
            {
                var medicationInteractions = await _repository.GetByMedicationIdAsync(medication.Id);
                foreach (var interaction in medicationInteractions)
                {
                    // Check if the interacting drug name matches any of the other medications in the list
                    if (medications.Any(m => m.Id != medication.Id && 
                        (m.Name.Equals(interaction.InteractingDrugName, StringComparison.OrdinalIgnoreCase) ||
                         m.GenericName.Equals(interaction.InteractingDrugName, StringComparison.OrdinalIgnoreCase))))
                    {
                        crossInteractions.Add(interaction);
                    }
                }
            }

            // Combine and remove duplicates
            var allFoundInteractions = allInteractions.Union(crossInteractions).Distinct();

            return _mapper.Map<IEnumerable<InteractionResponseDto>>(allFoundInteractions);
        }

        public async Task<InteractionResponseDto> CreateInteractionAsync(InteractionCreateDto createDto)
        {
            var interaction = _mapper.Map<DrugInteraction>(createDto);
            interaction.Id = Guid.NewGuid();
            interaction.DetectedAt = DateTime.UtcNow;
            interaction.IsAcknowledged = false;

            var created = await _repository.AddAsync(interaction);
            return _mapper.Map<InteractionResponseDto>(created);
        }

        public async Task<InteractionResponseDto?> UpdateInteractionAsync(Guid id, InteractionUpdateDto updateDto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return null;

            _mapper.Map(updateDto, existing);

            await _repository.UpdateAsync(existing);
            return _mapper.Map<InteractionResponseDto>(existing);
        }

        public async Task<bool> DeleteInteractionAsync(Guid id)
        {
            var interaction = await _repository.GetByIdAsync(id);
            if (interaction == null)
                return false;

            await _repository.DeleteAsync(interaction);
            return true;
        }
    }
}

