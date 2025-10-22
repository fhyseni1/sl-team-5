using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Relationships;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IUserRelationshipService
    {
        Task<UserRelationshipResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserRelationshipResponseDto>> GetAllAsync();
        Task<IEnumerable<UserRelationshipResponseDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserRelationshipResponseDto>> GetCaregiversByUserIdAsync(Guid userId);
        Task<UserRelationshipResponseDto> AddAsync(UserRelationshipCreateDto relationship);
        Task<UserRelationshipResponseDto> UpdateAsync(Guid id,UserRelationshipUpdateDto relationship);
        Task<UserRelationshipResponseDto?> UpdatePermissionsAsync(Guid id, string permissions);
        Task<bool> DeleteAsync(Guid id);
    }
}
