using AutoMapper;
using MedicationService.Application.DTOs.Prescriptions;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicationService.Application.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _repository;
        private readonly IMapper _mapper;

        public PrescriptionService(IPrescriptionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PrescriptionResponseDto> CreateAsync(PrescriptionCreateDto createDto)
        {
            var entity = _mapper.Map<Prescription>(createDto);
            await _repository.AddAsync(entity);
            return _mapper.Map<PrescriptionResponseDto>(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            await _repository.DeleteAsync(existing);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _repository.ExistsAsync(id);
        }

        public async Task<IEnumerable<PrescriptionResponseDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PrescriptionResponseDto>>(list);
        }

        public async Task<PrescriptionResponseDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            return _mapper.Map<PrescriptionResponseDto>(entity);
        }

        public async Task<IEnumerable<PrescriptionResponseDto>> GetByMedicationIdAsync(Guid medicationId)
        {
            var list = await _repository.GetByMedicationIdAsync(medicationId);
            return _mapper.Map<IEnumerable<PrescriptionResponseDto>>(list);
        }

        public async Task<PrescriptionResponseDto?> GetByPrescriptionNumberAsync(string prescriptionNumber)
        {
            var entity = await _repository.GetByPrescriptionNumberAsync(prescriptionNumber);
            return entity == null ? null : _mapper.Map<PrescriptionResponseDto>(entity);
        }

        public async Task<IEnumerable<PrescriptionResponseDto>> GetExpiringPrescriptionsAsync(DateTime beforeDate)
        {
            var list = await _repository.GetExpiringPrescriptionsAsync(beforeDate);
            return _mapper.Map<IEnumerable<PrescriptionResponseDto>>(list);
        }

        public async Task<IEnumerable<PrescriptionResponseDto>> GetLowRefillPrescriptionsAsync(int maxRefills)
        {
            var list = await _repository.GetLowRefillPrescriptionsAsync(maxRefills);
            return _mapper.Map<IEnumerable<PrescriptionResponseDto>>(list);
        }

        public async Task<PrescriptionResponseDto?> UpdateAsync(Guid id, PrescriptionUpdateDto updateDto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return null;

            _mapper.Map(updateDto, existing);

            await _repository.UpdateAsync(existing);
            return _mapper.Map<PrescriptionResponseDto>(existing);
        }
        public async Task<IEnumerable<PrescriptionResponseDto>> GetExpiringSoonAsync(int days = 30)
        {
            var prescriptions = await _repository.GetExpiringSoonAsync(days);

           return prescriptions.Select(p => new PrescriptionResponseDto
            {
                Id = p.Id,
                ExpiryDate = p.ExpiryDate,
                MedicationName = p.Medication.Name,  
                PrescriberName = p.PrescriberName,
                PharmacyName = p.PharmacyName,
            });

        }

    }
}
