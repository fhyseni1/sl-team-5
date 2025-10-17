using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using MedicationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicationService.Infrastructure.Repositories
{
    public class DrugInteractionRepository : IDrugInteractionRepository
    {
        private readonly MedicationDbContext _context;

        public DrugInteractionRepository(MedicationDbContext context)
        {
            _context = context;
        }

        public async Task<DrugInteraction?> GetByIdAsync(Guid id)
        {
            return await _context.DrugInteractions
                .Include(di => di.Medication)
                .FirstOrDefaultAsync(di => di.Id == id);
        }

        public async Task<IEnumerable<DrugInteraction>> GetAllAsync()
        {
            return await _context.DrugInteractions
                .Include(di => di.Medication)
                .OrderByDescending(di => di.DetectedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DrugInteraction>> FindAsync(System.Linq.Expressions.Expression<Func<DrugInteraction, bool>> predicate)
        {
            return await _context.DrugInteractions
                .Include(di => di.Medication)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<DrugInteraction> AddAsync(DrugInteraction entity)
        {
            entity.DetectedAt = DateTime.UtcNow;
            await _context.DrugInteractions.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(DrugInteraction entity)
        {
            _context.DrugInteractions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(DrugInteraction entity)
        {
            _context.DrugInteractions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.DrugInteractions.AnyAsync(di => di.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.DrugInteractions.CountAsync();
        }

        public async Task<IEnumerable<DrugInteraction>> GetByMedicationIdAsync(Guid medicationId)
        {
            return await _context.DrugInteractions
                .Include(di => di.Medication)
                .Where(di => di.MedicationId == medicationId)
                .OrderByDescending(di => di.Severity)
                .ThenByDescending(di => di.DetectedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DrugInteraction>> GetBySeverityAsync(InteractionSeverity severity)
        {
            return await _context.DrugInteractions
                .Include(di => di.Medication)
                .Where(di => di.Severity == severity)
                .OrderByDescending(di => di.DetectedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DrugInteraction>> GetUnacknowledgedAsync()
        {
            return await _context.DrugInteractions
                .Include(di => di.Medication)
                .Where(di => !di.IsAcknowledged)
                .OrderByDescending(di => di.Severity)
                .ThenByDescending(di => di.DetectedAt)
                .ToListAsync();
        }

        public async Task<DrugInteraction?> GetByMedicationAndDrugAsync(Guid medicationId, string drugName)
        {
            return await _context.DrugInteractions
                .Include(di => di.Medication)
                .FirstOrDefaultAsync(di => di.MedicationId == medicationId && 
                                          di.InteractingDrugName.ToLower() == drugName.ToLower());
        }
    }
}

