using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Relationships;
using UserHealthService.Application.Interfaces; 
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class UserRelationshipRepository : IUserRelationshipRepository
    {
        private readonly UserHealthDbContext _dbContext;
        public UserRelationshipRepository(UserHealthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserRelationship> AddAsync(UserRelationship relationship)
        {
            relationship.CreatedAt = DateTime.UtcNow;
            relationship.UpdatedAt = DateTime.UtcNow;
            relationship.IsActive = true;

            await _dbContext.UserRelationships.AddAsync(relationship);
            await _dbContext.SaveChangesAsync();

            return relationship;
        }

        public async Task<bool> DeleteAsync(UserRelationship relationship)
        {
            _dbContext.UserRelationships.Remove(relationship);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserRelationship>> GetAllAsync()
        {
            var allRelations = await _dbContext.UserRelationships.Include(e => e.User)
                .Include(e => e.RelatedUser).OrderByDescending(e => e.Id).ToListAsync();
            return allRelations;
        }

        public async Task<UserRelationship?> GetByIdAsync(Guid id)
        {
            var userRelationById = await _dbContext.UserRelationships.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            return userRelationById;
        }

        public async Task<IEnumerable<UserRelationship>> GetByUserIdAsync(Guid userId)
        {
            var usersById = await _dbContext.UserRelationships.Include(e => e.User).Where(e => e.UserId == userId && e.IsActive).ToListAsync();
            return usersById;
        }

        public async Task<IEnumerable<UserRelationship>> GetCaregiversByUserIdAsync(Guid userId)
        {
            var caregivers = await _dbContext.UserRelationships
                  .Include(r => r.User)
                  .Include(r => r.RelatedUser)
                  .Where(r => r.UserId == userId && r.IsActive)
                  .ToListAsync();
            return caregivers;
        }

        public async Task<UserRelationship> UpdateAsync(UserRelationship relationship)
        {
            relationship.UpdatedAt = DateTime.UtcNow;

            _dbContext.UserRelationships.Update(relationship);

            await _dbContext.SaveChangesAsync();

            return relationship;
        }

        public async Task<UserRelationship?> UpdatePermissionsAsync(Guid id, string permissions)
        {
            var relationship = await _dbContext.UserRelationships.FindAsync(id);
            if (relationship == null) return null;

            relationship.CanManageMedications = false;
            relationship.CanViewHealthData = false;
            relationship.CanScheduleAppointments = false;

            var perms = permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var perm in perms)
            {
                switch (perm)
                {
                    case "CanManageMedications":
                        relationship.CanManageMedications = true;
                        break;
                    case "CanViewHealthData":
                        relationship.CanViewHealthData = true;
                        break;
                    case "CanScheduleAppointments":
                        relationship.CanScheduleAppointments = true;
                        break;
                }
            }

            relationship.UpdatedAt = DateTime.UtcNow;

            _dbContext.UserRelationships.Update(relationship);
            await _dbContext.SaveChangesAsync();

            return relationship;
        }
    }
}