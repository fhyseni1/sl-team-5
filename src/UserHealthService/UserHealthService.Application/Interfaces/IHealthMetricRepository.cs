using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Interfaces
{
    public interface IHealthMetricRepository
    {
        Task<IEnumerable<HealthMetric>> GetAllAsync();
        Task<HealthMetric?> GetByIdAsync(Guid id);
        Task<IEnumerable<HealthMetric>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<HealthMetric>> GetByUserAndTypeAsync(Guid userId, HealthMetricType type);
        Task<IEnumerable<IGrouping<HealthMetricType, HealthMetric>>> GetUserTrendsAsync(Guid userId);
        Task<HealthMetric> AddAsync(HealthMetric metric);
        Task UpdateAsync(HealthMetric metric);
        Task DeleteAsync(HealthMetric metric);
        Task SaveChangesAsync();
    }
}
