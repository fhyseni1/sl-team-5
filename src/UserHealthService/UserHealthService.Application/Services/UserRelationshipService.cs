using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Notifications;
using UserHealthService.Application.DTOs.Relationships;
using UserHealthService.Application.DTOs.Symptoms;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Services
{
    public class UserRelationshipService : IUserRelationshipService
    {
        private readonly IUserRelationshipRepository _userRelationsRepository;
        private readonly IMapper _mapper;
        public UserRelationshipService(IUserRelationshipRepository userRelationsRepository, IMapper mapper)
        {
            _userRelationsRepository = userRelationsRepository;
            _mapper = mapper;
        }
        public async Task<UserRelationshipResponseDto> AddAsync(UserRelationshipCreateDto relationship)
        {
            var userRelation = _mapper.Map<UserRelationship>(relationship);
            var result = await _userRelationsRepository.AddAsync(userRelation);
            return _mapper.Map<UserRelationshipResponseDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _userRelationsRepository.GetByIdAsync(id);
            if (existing == null) return false;
            await _userRelationsRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<IEnumerable<UserRelationshipResponseDto>> GetAllAsync()
        {
            var allUserRelations = await _userRelationsRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserRelationshipResponseDto>>(allUserRelations);

        }

        public async Task<UserRelationshipResponseDto?> GetByIdAsync(Guid id)
        {
            var userRelationShipById = await _userRelationsRepository.GetByIdAsync(id);
            return userRelationShipById == null ? null : _mapper.Map<UserRelationshipResponseDto>(userRelationShipById);
        }

        public async Task<IEnumerable<UserRelationshipResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var byUserId = await _userRelationsRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<UserRelationshipResponseDto>>(byUserId);

        }

        public async Task<IEnumerable<UserRelationshipResponseDto>> GetCaregiversByUserIdAsync(Guid userId)
        {
            var caregivers = await _userRelationsRepository.GetCaregiversByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<UserRelationshipResponseDto>>(caregivers);
        }

        public async Task<UserRelationshipResponseDto> UpdateAsync(Guid id, UserRelationshipUpdateDto relationship)
        {
            var existing = await _userRelationsRepository.GetByIdAsync(id);
            if (existing == null) return null;
            _mapper.Map(relationship, existing);
            var updated = await _userRelationsRepository.UpdateAsync(existing);
            return _mapper.Map<UserRelationshipResponseDto>(updated);

        }

        public async Task<UserRelationshipResponseDto?> UpdatePermissionsAsync(Guid id, string permissions)
        {
            var updatedEntity = await _userRelationsRepository.UpdatePermissionsAsync(id, permissions);

            if (updatedEntity == null)
                return null;

            return _mapper.Map<UserRelationshipResponseDto>(updatedEntity);
        }
    }

}