using AutoMapper;
using MedicationService.Application.DTOs.Medications;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _repository;
        private readonly IMapper _mapper;

        public MedicationService(IMedicationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MedicationResponseDto?> GetByIdAsync(Guid id)
        {
            var medication = await _repository.GetByIdAsync(id);
            return medication == null ? null : _mapper.Map<MedicationResponseDto>(medication);
        }

        public async Task<IEnumerable<MedicationResponseDto>> GetAllAsync()
        {
            var medications = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MedicationResponseDto>>(medications);
        }

        public async Task<IEnumerable<MedicationResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var medications = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<MedicationResponseDto>>(medications);
        }

        public async Task<IEnumerable<MedicationResponseDto>> GetActiveByUserIdAsync(Guid userId)
        {
            var medications = await _repository.GetActiveByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<MedicationResponseDto>>(medications);
        }

        public async Task<MedicationResponseDto?> GetByIdWithDetailsAsync(Guid id)
        {
            var medication = await _repository.GetByIdWithDetailsAsync(id);
            return medication == null ? null : _mapper.Map<MedicationResponseDto>(medication);
        }

        public async Task<IEnumerable<MedicationResponseDto>> SearchByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<MedicationResponseDto>();

            var medications = await _repository.SearchByNameAsync(name);
            return _mapper.Map<IEnumerable<MedicationResponseDto>>(medications);
        }

        public async Task<MedicationResponseDto> CreateAsync(MedicationCreateDto createDto)
        {
            var medication = _mapper.Map<Medication>(createDto);
            medication.Id = Guid.NewGuid();
            medication.Status = MedicationStatus.Active;
            medication.CreatedAt = DateTime.UtcNow;
            medication.UpdatedAt = DateTime.UtcNow;

            var created = await _repository.AddAsync(medication);
            return _mapper.Map<MedicationResponseDto>(created);
        }

        public async Task<MedicationResponseDto?> UpdateAsync(Guid id, MedicationUpdateDto updateDto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return null;

            _mapper.Map(updateDto, existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return _mapper.Map<MedicationResponseDto>(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var medication = await _repository.GetByIdAsync(id);
            if (medication == null)
                return false;

            await _repository.DeleteAsync(medication);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}

