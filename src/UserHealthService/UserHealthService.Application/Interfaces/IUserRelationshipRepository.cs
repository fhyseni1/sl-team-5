using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Relationships;
using UserHealthService.Application.DTOs.Symptoms;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IUserRelationshipRepository
    {

        Task<UserRelationship?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserRelationship>> GetAllAsync();
        Task<IEnumerable<UserRelationship>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserRelationship>> GetCaregiversByUserIdAsync(Guid userId);
        Task<UserRelationship> AddAsync(UserRelationship relationship);
        Task<UserRelationship> UpdateAsync(UserRelationship relationship);
        Task<UserRelationship?> UpdatePermissionsAsync(Guid id, string permissions);
        Task<bool> DeleteAsync(UserRelationship relationship);
    }
}