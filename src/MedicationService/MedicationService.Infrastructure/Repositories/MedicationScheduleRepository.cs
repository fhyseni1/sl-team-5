using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using MedicationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Infrastructure.Repositories
{
    public class MedicationScheduleRepository : IMedicationScheduleRepository
    {
        public readonly MedicationDbContext _dbContext;
        public MedicationScheduleRepository(MedicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<MedicationSchedule> AddAsync(MedicationSchedule entity)
        {
            var medicationSchedule = new MedicationSchedule
            {
                Id = Guid.NewGuid(),
                MedicationId = entity.MedicationId,
                Frequency = entity.Frequency,
                CustomFrequencyHours = entity.CustomFrequencyHours,
                TimeOfDay = entity.TimeOfDay,
                DaysOfWeek = entity.DaysOfWeek,
                IsActive = entity.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.MedicationSchedules.Add(medicationSchedule);
            await _dbContext.SaveChangesAsync();

            return medicationSchedule;
        }

        public async Task<int> CountAsync()
        {
            return await _dbContext.MedicationSchedules.CountAsync();
        }

        public async Task DeleteAsync(MedicationSchedule entity)
        {
            _dbContext.MedicationSchedules.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.MedicationSchedules.AnyAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MedicationSchedule>> FindAsync(Expression<Func<MedicationSchedule, bool>> predicate)
        {
            return await _dbContext.MedicationSchedules.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<MedicationSchedule>> GetActiveSchedulesAsync()
        {
            return await _dbContext.MedicationSchedules
         .Where(e => e.IsActive) 
         .ToListAsync();
        }

        public async Task<IEnumerable<MedicationSchedule>> GetAllAsync()
        {
            return await _dbContext.MedicationSchedules.ToListAsync();
        }

        public async Task<MedicationSchedule?> GetByIdAsync(Guid id)
        {
            return await _dbContext.MedicationSchedules.FindAsync(id);
        }

        public async Task<IEnumerable<MedicationSchedule>> GetByMedicationIdAsync(Guid medicationId)
        {
            return await _dbContext.MedicationSchedules
                 .Include(e => e.Medication)
               .Where(m => m.MedicationId == medicationId)
               .OrderByDescending(m => m.CreatedAt)
               .ToListAsync();
        }

        public async Task<IEnumerable<MedicationSchedule>> GetSchedulesByFrequencyAsync(FrequencyType frequency)
        {
            return await _dbContext.MedicationSchedules
               .Where(m => m.Frequency == frequency)
               .OrderByDescending(m => m.CreatedAt)
               .ToListAsync();
        }

        public async Task<IEnumerable<MedicationSchedule>> GetUpcomingSchedulesAsync(DateTime startTime, DateTime endTime)
        {
            var schedules = await _dbContext.MedicationSchedules
        .Include(s => s.Medication)
        .Where(s => s.IsActive)
        .ToListAsync();
            var upcoming = schedules.Where(s =>
            {
                var candidate = startTime.Date.Add(s.TimeOfDay);
                if (candidate < startTime)
                {
                    candidate = candidate.AddDays(1);
                }

                return candidate >= startTime && candidate <= endTime;
            });
            return upcoming;
        }

        public async Task UpdateAsync(MedicationSchedule entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbContext.MedicationSchedules.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
