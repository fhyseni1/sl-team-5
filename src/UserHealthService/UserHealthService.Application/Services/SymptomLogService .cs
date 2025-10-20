using AutoMapper;
using UserHealthService.Application.DTOs.Symptoms;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Services
{
    public class SymptomLogService : ISymptomLogService
    {
        private readonly ISymptomLogRepository _symptomLogRepository;
        private readonly IMapper _mapper;

        public SymptomLogService(ISymptomLogRepository symptomLogRepository, IMapper mapper)
        {
            _symptomLogRepository = symptomLogRepository;
            _mapper = mapper;
        }

        public async Task<SymptomLogResponseDto?> GetByIdAsync(Guid id)
        {
            var symptomLog = await _symptomLogRepository.GetByIdAsync(id);
            return symptomLog == null ? null : _mapper.Map<SymptomLogResponseDto>(symptomLog);
        }

        public async Task<IEnumerable<SymptomLogResponseDto>> GetAllAsync()
        {
            var symptomLogs = await _symptomLogRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<SymptomLogResponseDto>>(symptomLogs);
        }

        public async Task<IEnumerable<SymptomLogResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var symptomLogs = await _symptomLogRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<SymptomLogResponseDto>>(symptomLogs);
        }

        public async Task<IEnumerable<SymptomLogResponseDto>> GetByUserIdAndSeverityAsync(Guid userId, SymptomSeverity severity)
        {
            var symptomLogs = await _symptomLogRepository.GetByUserIdAndSeverityAsync(userId, severity);
            return _mapper.Map<IEnumerable<SymptomLogResponseDto>>(symptomLogs);
        }

        public async Task<SymptomLogResponseDto> CreateAsync(SymptomLogCreateDto createDto)
        {
            var symptomLog = _mapper.Map<SymptomLog>(createDto);
            var result = await _symptomLogRepository.AddAsync(symptomLog);
            return _mapper.Map<SymptomLogResponseDto>(result);
        }

        public async Task<SymptomLogResponseDto?> UpdateAsync(Guid id, SymptomLogUpdateDto updateDto)
        {
            var existingSymptomLog = await _symptomLogRepository.GetByIdAsync(id);
            if (existingSymptomLog == null)
                return null;

            _mapper.Map(updateDto, existingSymptomLog);
            var result = await _symptomLogRepository.UpdateAsync(existingSymptomLog);
            return _mapper.Map<SymptomLogResponseDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _symptomLogRepository.DeleteAsync(id);
        }
    }
}