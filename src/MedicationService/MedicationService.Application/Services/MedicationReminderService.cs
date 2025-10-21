using AutoMapper;
using MedicationService.Application.DTOs.Doses;
using MedicationService.Application.DTOs.Reminders;
using MedicationService.Application.DTOs.Schedules;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.Services
{

    public class MedicationReminderService : IMedicationReminderService
    {
        private readonly IMedicationReminderRepository _repository;
        private readonly IMapper _mapper;

        public MedicationReminderService(IMedicationReminderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ReminderResponseDto> CreateAsync(ReminderCreateDto dto)
        {
            var reminder = _mapper.Map<MedicationReminder>(dto);
            reminder.Id = Guid.NewGuid();
            reminder.Status = ReminderStatus.Scheduled;
            reminder.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(reminder);

            return _mapper.Map<ReminderResponseDto>(reminder);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(existing);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _repository.ExistsAsync(id);

        public async Task<IEnumerable<ReminderResponseDto>> GetAllAsync()
            => _mapper.Map<IEnumerable<ReminderResponseDto>>(await _repository.GetAllAsync());

        public async Task<ReminderResponseDto?> GetByIdAsync(Guid id)
            => _mapper.Map<ReminderResponseDto>(await _repository.GetByIdAsync(id));

        public async Task<IEnumerable<ReminderResponseDto>> GetByMedicationIdAsync(Guid medicationId)
            => _mapper.Map<IEnumerable<ReminderResponseDto>>(await _repository.GetByMedicationIdAsync(medicationId));

        public async Task<IEnumerable<ReminderResponseDto>> GetByStatusAsync(ReminderStatus status)
            => _mapper.Map<IEnumerable<ReminderResponseDto>>(await _repository.GetByStatusAsync(status));

        public async Task<IEnumerable<ReminderResponseDto>> GetMissedRemindersAsync()
            => _mapper.Map<IEnumerable<ReminderResponseDto>>(await _repository.GetMissedRemindersAsync());

        public async Task<IEnumerable<ReminderResponseDto>> GetPendingRemindersAsync()
            => _mapper.Map<IEnumerable<ReminderResponseDto>>(await _repository.GetPendingRemindersAsync());


        public async Task<IEnumerable<ReminderResponseDto>> GetUpcomingRemindersAsync(DateTime beforeTime)
            => _mapper.Map<IEnumerable<ReminderResponseDto>>(await _repository.GetUpcomingRemindersAsync(beforeTime));

        public async Task<ReminderResponseDto?> SnoozeReminder(Guid id, ReminderUpdateDto dto)
        {
            var existingReminder = await _repository.GetByIdAsync(id);
            if (existingReminder == null)
                return null;


            var newReminderTime = DateTime.UtcNow.AddMinutes(10);


            existingReminder.ScheduledTime = newReminderTime;
            existingReminder.Status = ReminderStatus.Snoozed;
            existingReminder.SnoozeCount += 1;


            await _repository.UpdateAsync(existingReminder);

            return _mapper.Map<ReminderResponseDto>(existingReminder);
        }

        public async Task<ReminderResponseDto?> UpdateAsync(Guid id, ReminderUpdateDto dto)

        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);

            if (existing.Status == ReminderStatus.Scheduled && existing.ScheduledTime < DateTime.UtcNow)
                existing.Status = ReminderStatus.Missed;
            else if (existing.ScheduledTime > DateTime.UtcNow)
            {
                existing.Status = ReminderStatus.Scheduled;
            }

            await _repository.UpdateAsync(existing);

            return _mapper.Map<ReminderResponseDto>(existing);
        }
    }

}