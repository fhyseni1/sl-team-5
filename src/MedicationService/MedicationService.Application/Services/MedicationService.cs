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
        private readonly IMedicationScheduleService _scheduleService;
        private readonly IScheduleGeneratorService _scheduleGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public MedicationService(
            IMedicationRepository repository, 
            IMapper mapper,
            IMedicationScheduleService scheduleService,
            IScheduleGeneratorService scheduleGenerator,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _mapper = mapper;
            _scheduleService = scheduleService;
            _scheduleGenerator = scheduleGenerator;
            _unitOfWork = unitOfWork;
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

        public async Task<IEnumerable<MedicationResponseDto>> GetMedicationsByStatusAsync(MedicationStatus status)
        {
            var medications = await _repository.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<MedicationResponseDto>>(medications);
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
            try
            {
                if (createDto.Frequency.HasValue)
                {
                    ValidateFrequencyInputs(createDto.Frequency.Value, createDto.CustomFrequencyHours, createDto.DaysOfWeek, createDto.MonthlyDay);
                }

                await _unitOfWork.BeginTransactionAsync();

                var medication = _mapper.Map<Medication>(createDto);
                medication.Id = Guid.NewGuid();
                medication.Status = MedicationStatus.Active;
                medication.CreatedAt = DateTime.UtcNow;
                medication.UpdatedAt = DateTime.UtcNow;

                var created = await _repository.AddAsync(medication);

                var scheduleIds = new List<Guid>();

                if (createDto.Frequency.HasValue)
                {
                    var scheduleDtos = _scheduleGenerator.GenerateSchedules(
                        created.Id,
                        createDto.Frequency.Value,
                        createDto.CustomFrequencyHours,
                        createDto.DaysOfWeek,
                        createDto.MonthlyDay);

                    foreach (var scheduleDto in scheduleDtos)
                    {
                        var schedule = await _scheduleService.CreateAsync(scheduleDto);
                        scheduleIds.Add(schedule.Id);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                var response = _mapper.Map<MedicationResponseDto>(created);
                response.ScheduleIds = scheduleIds;
                return response;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private void ValidateFrequencyInputs(
            FrequencyType frequency,
            int? customFrequencyHours,
            string? daysOfWeek,
            int? monthlyDay)
        {
            if (frequency == FrequencyType.Custom || frequency == FrequencyType.EveryFewHours)
            {
                if (!customFrequencyHours.HasValue || customFrequencyHours.Value <= 0)
                {
                    throw new ArgumentException(
                        $"CustomFrequencyHours is required and must be greater than 0 for frequency type {frequency}.",
                        nameof(customFrequencyHours));
                }
            }

            if (frequency == FrequencyType.Weekly)
            {
                if (string.IsNullOrWhiteSpace(daysOfWeek))
                {
                    throw new ArgumentException(
                        "DaysOfWeek is required for Weekly frequency.",
                        nameof(daysOfWeek));
                }
            }

            if (frequency == FrequencyType.Monthly)
            {
                if (!monthlyDay.HasValue || monthlyDay.Value < 1 || monthlyDay.Value > 31)
                {
                    throw new ArgumentException(
                        "MonthlyDay is required and must be between 1 and 31 for Monthly frequency.",
                        nameof(monthlyDay));
                }
            }
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

        public async Task<IEnumerable<MedicationResponseDto>> SearchMedicationsAsync(string searchTerm)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return Enumerable.Empty<MedicationResponseDto>();

                var medications = await _repository.SearchByNameAsync(searchTerm);
                return _mapper.Map<IEnumerable<MedicationResponseDto>>(medications);
            }

    }
}

