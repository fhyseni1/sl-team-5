using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class AllergyRepository : IAllergyRepository
    {
        private readonly UserHealthDbContext _context;

        public AllergyRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<Allergy?> GetByIdAsync(Guid id)
        {
            return await _context.Allergies.FindAsync(id);
        }

        public async Task<IEnumerable<Allergy>> GetAllAsync()
        {
            return await _context.Allergies
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Allergy>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Allergies
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Allergies
                .CountAsync(a => a.UserId == userId && a.IsActive);
        }

        public async Task<Allergy> AddAsync(Allergy allergy)
        {
            allergy.CreatedAt = DateTime.UtcNow;
            allergy.UpdatedAt = DateTime.UtcNow;
            
            _context.Allergies.Add(allergy);
            await _context.SaveChangesAsync();
            return allergy;
        }

        public async Task<Allergy> UpdateAsync(Allergy allergy)
        {
            allergy.UpdatedAt = DateTime.UtcNow;
            _context.Allergies.Update(allergy);
            await _context.SaveChangesAsync();
            return allergy;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy == null) return false;

            allergy.IsActive = false;
            allergy.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Allergies.AnyAsync(a => a.Id == id && a.IsActive);
        }
    }
}