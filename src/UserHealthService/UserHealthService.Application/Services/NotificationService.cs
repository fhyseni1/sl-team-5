using AutoMapper;
using UserHealthService.Application.DTOs.Notifications;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetAllAsync()
        {
            var notifications = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);
        }

        public async Task<NotificationResponseDto?> GetByIdAsync(Guid id)
        {
            var notification = await _repository.GetByIdAsync(id);
            return notification == null ? null : _mapper.Map<NotificationResponseDto>(notification);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var notifications = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);
        }

        public async Task<IEnumerable<NotificationResponseDto>> GetUnreadByUserIdAsync(Guid userId)
        {
            var notifications = await _repository.GetUnreadByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);
        }

        public async Task<NotificationResponseDto> CreateAsync(NotificationCreateDto dto)
        {
            var entity = _mapper.Map<Notification>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return _mapper.Map<NotificationResponseDto>(entity);
        }

        public async Task<bool> MarkAsReadAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsRead = true;
            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
