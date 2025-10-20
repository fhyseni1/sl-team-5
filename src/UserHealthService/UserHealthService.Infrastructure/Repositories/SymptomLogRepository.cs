using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class SymptomLogRepository : ISymptomLogRepository
    {
        private readonly UserHealthDbContext _context;

        public SymptomLogRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<SymptomLog?> GetByIdAsync(Guid id)
        {
            return await _context.SymptomLogs
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<SymptomLog>> GetAllAsync()
        {
            return await _context.SymptomLogs
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SymptomLog>> GetByUserIdAsync(Guid userId)
        {
            return await _context.SymptomLogs
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SymptomLog>> GetByUserIdAndSeverityAsync(Guid userId, SymptomSeverity severity)
        {
            return await _context.SymptomLogs
                .Where(s => s.UserId == userId && s.Severity == severity)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<SymptomLog> AddAsync(SymptomLog symptomLog)
        {
            symptomLog.CreatedAt = DateTime.UtcNow;
            _context.SymptomLogs.Add(symptomLog);
            await _context.SaveChangesAsync();
            return symptomLog;
        }

        public async Task<SymptomLog> UpdateAsync(SymptomLog symptomLog)
        {
            _context.SymptomLogs.Update(symptomLog);
            await _context.SaveChangesAsync();
            return symptomLog;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var symptomLog = await GetByIdAsync(id);
            if (symptomLog == null)
                return false;

            _context.SymptomLogs.Remove(symptomLog);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}