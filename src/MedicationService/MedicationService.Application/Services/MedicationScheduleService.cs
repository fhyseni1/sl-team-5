using AutoMapper;
using MedicationService.Application.DTOs.Schedules;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;

namespace MedicationService.Application.Services
{
    public class MedicationScheduleService : IMedicationScheduleService
    {
        private readonly IMedicationScheduleRepository _repository;
        private readonly IMapper _mapper;

        public MedicationScheduleService(IMedicationScheduleRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ScheduleResponseDto> CreateAsync(ScheduleCreateDto dto)
        {
            var schedule = _mapper.Map<MedicationSchedule>(dto);
            schedule.Id = Guid.NewGuid();
            schedule.IsActive = true;
            schedule.CreatedAt = DateTime.UtcNow;
            schedule.UpdatedAt = DateTime.UtcNow;

            var createdSchedule = await _repository.AddAsync(schedule);
            return _mapper.Map<ScheduleResponseDto>(createdSchedule);
        }

        public async Task<IEnumerable<ScheduleResponseDto>> GetAllAsync()
        {
            var schedules = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ScheduleResponseDto>>(schedules);
        }

        public async Task<IEnumerable<ScheduleResponseDto>> GetActiveSchedulesAsync()
        {
            var activeSchedules = await _repository.GetActiveSchedulesAsync();
            return _mapper.Map<IEnumerable<ScheduleResponseDto>>(activeSchedules);
        }

        public async Task<IEnumerable<ScheduleResponseDto>> GetByMedicationIdAsync(Guid medicationId)
        {
            var schedules = await _repository.GetByMedicationIdAsync(medicationId);
            return _mapper.Map<IEnumerable<ScheduleResponseDto>>(schedules);
        }

        public async Task<IEnumerable<ScheduleResponseDto>> GetByFrequencyAsync(FrequencyType frequency)
        {
            var schedules = await _repository.GetSchedulesByFrequencyAsync(frequency);
            return _mapper.Map<IEnumerable<ScheduleResponseDto>>(schedules);
        }

        public async Task<ScheduleResponseDto?> GetByIdAsync(Guid id)
        {
            var schedule = await _repository.GetByIdAsync(id);
            return schedule == null ? null : _mapper.Map<ScheduleResponseDto>(schedule);
        }

        public async Task<IEnumerable<ScheduleResponseDto>> GetUpcomingSchedulesAsync(DateTime startTime, DateTime endTime)
        {
            var schedules = await _repository.GetUpcomingSchedulesAsync(startTime, endTime);
            return _mapper.Map<IEnumerable<ScheduleResponseDto>>(schedules);
        }

        public async Task<ScheduleResponseDto?> UpdateAsync(Guid id, ScheduleUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return null;

            existing.Frequency = dto.Frequency;
            existing.CustomFrequencyHours = dto.CustomFrequencyHours;
            existing.TimeOfDay = dto.TimeOfDay;
            existing.DaysOfWeek = dto.DaysOfWeek;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return _mapper.Map<ScheduleResponseDto>(existing);
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
    }
}
