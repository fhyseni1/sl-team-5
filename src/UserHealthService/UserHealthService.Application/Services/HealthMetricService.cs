using AutoMapper;
using UserHealthService.Application.DTOs.HealthMetrics;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Services
{
    public class HealthMetricService : IHealthMetricService
    {
        private readonly IHealthMetricRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        

        public HealthMetricService(IHealthMetricRepository repository, IUserRepository userRepository, IMapper mapper)
        {
            _repository = repository;
            _userRepository = userRepository;
            _mapper = mapper;
            
        }

        public async Task<HealthMetricResponseDto?> GetLatestMetricAsync(Guid userId, HealthMetricType type)
        {
            var userExists = await _userRepository.ExistsAsync(userId);
            if (!userExists)
                return null;

            var metric = await _repository.GetLatestByUserAndTypeAsync(userId, type);
            if (metric == null)
                return null;

            return new HealthMetricResponseDto
            {
               Id = metric.Id,
                UserId = metric.UserId,
                Type = metric.Type,
                Value = metric.Value,
                Unit = metric.Unit,
                Notes = metric.Notes,
                Device = metric.Device,
                RecordedAt = metric.RecordedAt,
                CreatedAt = metric.CreatedAt
            };
        }

        public async Task<IEnumerable<HealthMetricResponseDto>> GetAllAsync()
        {
            var metrics = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<HealthMetricResponseDto>>(metrics);
        }

        public async Task<HealthMetricResponseDto?> GetByIdAsync(Guid id)
        {
            var metric = await _repository.GetByIdAsync(id);
            return _mapper.Map<HealthMetricResponseDto>(metric);
        }

        public async Task<IEnumerable<HealthMetricResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var metrics = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<HealthMetricResponseDto>>(metrics);
        }

        public async Task<IEnumerable<HealthMetricResponseDto>> GetByUserAndTypeAsync(Guid userId, string type)
        {
            if (!Enum.TryParse<HealthMetricType>(type, true, out var metricType))
                throw new ArgumentException("Invalid health metric type.");

            var metrics = await _repository.GetByUserAndTypeAsync(userId, metricType);
            return _mapper.Map<IEnumerable<HealthMetricResponseDto>>(metrics);
        }

        public async Task<object> GetUserTrendsAsync(Guid userId)
        {
            var grouped = await _repository.GetUserTrendsAsync(userId);
            return grouped.Select(g => new
            {
                Type = g.Key.ToString(),
                Average = g.Average(x => (double)x.Value),
                Count = g.Count()
            });
        }

        public async Task<HealthMetricResponseDto> CreateAsync(HealthMetricCreateDto dto)
        {
            var entity = _mapper.Map<HealthMetric>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return _mapper.Map<HealthMetricResponseDto>(entity);
        }

        public async Task<bool> UpdateAsync(Guid id, HealthMetricUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(existing);
            await _repository.SaveChangesAsync();
            return true;
        }
       
    }
}
