using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class HealthMetricRepository : IHealthMetricRepository
    {
        private readonly UserHealthDbContext _context;

        public HealthMetricRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HealthMetric>> GetAllAsync()
        {
            return await _context.HealthMetrics.AsNoTracking().ToListAsync();
        }

        public async Task<HealthMetric?> GetByIdAsync(Guid id)
        {
            return await _context.HealthMetrics.FindAsync(id);
        }

        public async Task<IEnumerable<HealthMetric>> GetByUserIdAsync(Guid userId)
        {
            return await _context.HealthMetrics
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<HealthMetric>> GetByUserAndTypeAsync(Guid userId, HealthMetricType type)
        {
            return await _context.HealthMetrics
                .Where(x => x.UserId == userId && x.Type == type)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<IGrouping<HealthMetricType, HealthMetric>>> GetUserTrendsAsync(Guid userId)
        {
            var metrics = await _context.HealthMetrics
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            return metrics.GroupBy(x => x.Type);
        }

        public async Task<HealthMetric> AddAsync(HealthMetric metric)
        {
            await _context.HealthMetrics.AddAsync(metric);
            return metric;
        }

        public async Task UpdateAsync(HealthMetric metric)
        {
            _context.HealthMetrics.Update(metric);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(HealthMetric metric)
        {
            _context.HealthMetrics.Remove(metric);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
