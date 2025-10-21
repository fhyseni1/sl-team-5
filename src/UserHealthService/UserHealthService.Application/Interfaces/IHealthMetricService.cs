using System;
using System.Threading.Tasks;
using UserHealthService.Domain.Enums; 
using UserHealthService.Application.DTOs.HealthMetrics;

namespace UserHealthService.Application.Interfaces
{
    public interface IHealthMetricService
    {
        Task<IEnumerable<HealthMetricResponseDto>> GetAllAsync();
        Task<HealthMetricResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<HealthMetricResponseDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<HealthMetricResponseDto>> GetByUserAndTypeAsync(Guid userId, string type);
        Task<object> GetUserTrendsAsync(Guid userId);
        Task<HealthMetricResponseDto> CreateAsync(HealthMetricCreateDto dto);
        Task<bool> UpdateAsync(Guid id, HealthMetricUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<HealthMetricResponseDto?> GetLatestMetricAsync(Guid userId, HealthMetricType type);
    }
}
