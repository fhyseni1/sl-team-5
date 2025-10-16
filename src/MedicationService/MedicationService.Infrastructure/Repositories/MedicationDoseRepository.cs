using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MedicationService.Infrastructure.Repositories
{
    public class MedicationDoseRepository : IMedicationDoseRepository
    {
        private readonly MedicationDbContext _context;

        public MedicationDoseRepository(MedicationDbContext context)
        {
            _context = context;
        }

        // IRepository Methods
        public async Task<IEnumerable<MedicationDose>> GetAllAsync()
        {
            return await _context.MedicationDoses
                .Include(d => d.Medication)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MedicationDose?> GetByIdAsync(Guid id)
        {
            return await _context.MedicationDoses
                .Include(d => d.Medication)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<MedicationDose>> FindAsync(Expression<Func<MedicationDose, bool>> predicate)
        {
            return await _context.MedicationDoses
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.MedicationDoses.AnyAsync(d => d.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.MedicationDoses.CountAsync();
        }

        public async Task<MedicationDose> AddAsync(MedicationDose dose)
        {
            await _context.MedicationDoses.AddAsync(dose);
            return dose;
        }

        public async Task UpdateAsync(MedicationDose dose)
        {
            _context.MedicationDoses.Update(dose);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(MedicationDose dose)
        {
            _context.MedicationDoses.Remove(dose);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // IMedicationDoseRepository Methods
        public async Task<IEnumerable<MedicationDose>> GetByMedicationIdAsync(Guid medicationId)
        {
            return await _context.MedicationDoses
                .Include(d => d.Medication)
                .Where(d => d.MedicationId == medicationId)
                .OrderBy(d => d.ScheduledTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicationDose>> GetTodaysDosesAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.MedicationDoses
                .Include(d => d.Medication)
                .Where(d => d.Medication.UserId == userId && d.ScheduledTime.Date == today)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicationDose>> GetMissedDosesAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            return await _context.MedicationDoses
                .Include(d => d.Medication)
                .Where(d => d.Medication.UserId == userId &&
                            !d.IsTaken &&
                            d.ScheduledTime < now)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicationDose>> GetDosesByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _context.MedicationDoses
                .Include(d => d.Medication)
                .Where(d => d.Medication.UserId == userId &&
                            d.ScheduledTime.Date >= startDate.Date &&
                            d.ScheduledTime.Date <= endDate.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetMissedDoseCountAsync(Guid medicationId)
        {
            var now = DateTime.UtcNow;
            return await _context.MedicationDoses
                .Where(d => d.MedicationId == medicationId &&
                            !d.IsTaken &&
                            d.ScheduledTime < now)
                .CountAsync();
        }
    }
}
