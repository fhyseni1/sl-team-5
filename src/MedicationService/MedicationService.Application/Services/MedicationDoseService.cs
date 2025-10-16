using AutoMapper;
using MedicationService.Application.DTOs;
using MedicationService.Application.DTOs.Doses;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicationService.Application.Services
{
    public class MedicationDoseService : IMedicationDoseService
    {
        private readonly IMedicationDoseRepository _repository;
        private readonly IMapper _mapper;

        public MedicationDoseService(IMedicationDoseRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DoseResponseDto>> GetAllAsync()
        {
            var doses = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<DoseResponseDto>>(doses);
        }

        public async Task<DoseResponseDto?> GetByIdAsync(Guid id)
        {
            var dose = await _repository.GetByIdAsync(id);
            return dose == null ? null : _mapper.Map<DoseResponseDto>(dose);
        }

        public async Task<IEnumerable<DoseResponseDto>> GetByMedicationIdAsync(Guid medicationId)
        {
            var doses = await _repository.GetByMedicationIdAsync(medicationId);
            return _mapper.Map<IEnumerable<DoseResponseDto>>(doses);
        }

        public async Task<IEnumerable<DoseResponseDto>> GetTodayDosesAsync(Guid userId)
        {
            var doses = await _repository.GetTodaysDosesAsync(userId);
            return _mapper.Map<IEnumerable<DoseResponseDto>>(doses);
        }

        public async Task<IEnumerable<DoseResponseDto>> GetMissedDosesAsync(Guid userId)
        {
            var doses = await _repository.GetMissedDosesAsync(userId);
            return _mapper.Map<IEnumerable<DoseResponseDto>>(doses);
        }

        public async Task<DoseResponseDto> CreateAsync(DoseCreateDto dto)
        {
            var dose = _mapper.Map<MedicationDose>(dto);
            dose.Id = Guid.NewGuid();
            dose.CreatedAt = DateTime.UtcNow;
            dose.IsTaken = false;
            dose.IsMissed = false;

            await _repository.AddAsync(dose);
            await _repository.SaveChangesAsync();

            return _mapper.Map<DoseResponseDto>(dose);
        }

        public async Task<DoseResponseDto?> UpdateAsync(Guid id, DoseUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);

            // Update IsMissed based on new data
            if (!existing.IsTaken && existing.ScheduledTime < DateTime.UtcNow)
                existing.IsMissed = true;

            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();

            return _mapper.Map<DoseResponseDto>(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var dose = await _repository.GetByIdAsync(id);
            if (dose == null) return false;

            await _repository.DeleteAsync(dose);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
