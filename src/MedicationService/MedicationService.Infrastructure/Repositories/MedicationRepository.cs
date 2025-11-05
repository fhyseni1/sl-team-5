using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using MedicationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicationService.Infrastructure.Repositories
{
    public class MedicationRepository : IMedicationRepository
    {
        private readonly MedicationDbContext _context;

        public MedicationRepository(MedicationDbContext context)
        {
            _context = context;
        }

        public async Task<Medication?> GetByIdAsync(Guid id)
        {
            return await _context.Medications.FindAsync(id);
        }

        public async Task<IEnumerable<Medication>> GetAllAsync()
        {
            return await _context.Medications.ToListAsync();
        }

        public async Task<IEnumerable<Medication>> FindAsync(System.Linq.Expressions.Expression<Func<Medication, bool>> predicate)
        {
            return await _context.Medications.Where(predicate).ToListAsync();
        }

        public async Task<Medication> AddAsync(Medication entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.Medications.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Medication entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Medications.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Medication entity)
        {
            _context.Medications.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Medications.AnyAsync(m => m.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Medications.CountAsync();
        }

        public async Task<IEnumerable<Medication>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Medications
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.Medications
                .Where(m => m.UserId == userId && m.Status == MedicationStatus.Active)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Medication?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Medications
                .Include(m => m.Schedules)
                .Include(m => m.Doses)
                .Include(m => m.DrugInteractions)
                .Include(m => m.Prescriptions)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Medication>> GetByStatusAsync(MedicationStatus status)
        {
            return await _context.Medications
                .Where(m => m.Status == status)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> SearchByNameAsync(string name)
        {
            return await _context.Medications
                .Where(m => m.Name.Contains(name) || m.GenericName.Contains(name))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        
    }
}

