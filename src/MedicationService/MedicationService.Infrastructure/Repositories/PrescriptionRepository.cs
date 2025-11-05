using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
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
   
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly MedicationDbContext _context;
        public PrescriptionRepository(MedicationDbContext context)
        {
            _context = context;
        }
      public async Task<Prescription> AddAsync(Prescription entity)
{
    
    entity.IssueDate = entity.IssueDate.ToUniversalTime();
    
    if (entity.ExpiryDate.HasValue)
    {
        entity.ExpiryDate = entity.ExpiryDate.Value.ToUniversalTime();
    }
    else
    {
        entity.ExpiryDate = DateTime.UtcNow.AddDays(30); 
    }
    
    _context.Prescriptions.Add(entity);
    await _context.SaveChangesAsync();
    return entity;
}

        public async Task<int> CountAsync()
        {
            return await _context.Prescriptions.CountAsync();
        }

        public async Task DeleteAsync(Prescription entity)
        {
            _context.Prescriptions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Prescriptions.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Prescription>> FindAsync(Expression<Func<Prescription, bool>> predicate)
        {
            return await _context.Prescriptions.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<Prescription>> GetAllAsync()
        {
            return await _context.Prescriptions.ToListAsync();
        }

        public async Task<Prescription?> GetByIdAsync(Guid id)
        {
            return await  _context.Prescriptions.FindAsync(id);
        }

        public async Task<Prescription?> GetByMedicationIdAsync(Guid medicationId)
        {
            return await  _context.Prescriptions.FirstOrDefaultAsync(e => e.MedicationId == medicationId);
               
        }

        public async Task<Prescription?> GetByPrescriptionNumberAsync(string prescriptionNumber)
        {
            return await _context.Prescriptions.FirstOrDefaultAsync(p => p.PrescriptionNumber == prescriptionNumber);
        }

        public async Task<IEnumerable<Prescription>> GetExpiringPrescriptionsAsync(DateTime beforeDate)
        {
            return await _context.Prescriptions
         .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value <= beforeDate)
         .ToListAsync();
        }

        public async Task<IEnumerable<Prescription>> GetLowRefillPrescriptionsAsync(int maxRefills)
        {
            return await _context.Prescriptions
         .Where(p => p.RemainingRefills <= maxRefills && p.RemainingRefills > 0)
         .Include(p => p.Medication) 
         .ToListAsync();
        }

       public async Task UpdateAsync(Prescription entity)
{
   
    if (entity.ExpiryDate == null)
        entity.ExpiryDate = DateTime.UtcNow.AddDays(30);
    else
        entity.ExpiryDate = entity.ExpiryDate.Value.ToUniversalTime();
        
    _context.Prescriptions.Update(entity);
    await _context.SaveChangesAsync();
}
        public async Task<IEnumerable<Prescription>> GetExpiringSoonAsync(int days)
        {
            var now = DateTime.UtcNow;
            var targetDate = now.AddDays(days);

            return await _context.Prescriptions
                .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value >= now && p.ExpiryDate.Value <= targetDate)
                .Include(p => p.Medication)
                .ToListAsync();
        }

    }
}
