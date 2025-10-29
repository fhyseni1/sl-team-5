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
        private readonly IUserRelationshipService _relationshipService;

        public NotificationService(INotificationRepository repository, IMapper mapper, IUserRelationshipService relationshipService)
        {
            _repository = repository;
            _mapper = mapper;
            _relationshipService = relationshipService;
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
            await DistributeToCaregiversAsync(entity);
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

        public async Task<IEnumerable<NotificationResponseDto>> GetByCaregiverIdAsync(Guid caregiverId)
        {
            // Get all relationships and filter for ones where caregiverId is the RelatedUserId (caregiver)
            var allRelationships = await _relationshipService.GetAllAsync();
            var relationships = allRelationships.Where(r => r.RelatedUserId == caregiverId && r.IsActive);

            var allNotifications = new List<NotificationResponseDto>();

            foreach (var relationship in relationships)
            {
                var patientNotifications = await _repository.GetByUserIdAsync(relationship.UserId);
                var mappedNotifications = _mapper.Map<IEnumerable<NotificationResponseDto>>(patientNotifications);
                allNotifications.AddRange(mappedNotifications);
            }

            var caregiverDirectNotifications = await _repository.GetByUserIdAsync(caregiverId);
            var mappedCaregiverNotifications = _mapper.Map<IEnumerable<NotificationResponseDto>>(caregiverDirectNotifications);
            allNotifications.AddRange(mappedCaregiverNotifications);

            return allNotifications.OrderByDescending(n => n.CreatedAt).ToList();
        }

        private async Task DistributeToCaregiversAsync(Notification notification)
        {
            try
            {
                var relationships = await _relationshipService.GetCaregiversByUserIdAsync(notification.UserId);

                foreach (var relationship in relationships)
                {
                    if (!relationship.IsActive) continue;

                    var caregiverNotification = new Notification
                    {
                        UserId = relationship.RelatedUserId,
                        Type = notification.Type,
                        Title = $"[{relationship.UserName}] {notification.Title}",
                        Message = notification.Message,
                        ScheduledTime = notification.ScheduledTime,
                        ActionUrl = notification.ActionUrl,
                        Priority = notification.Priority,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _repository.AddAsync(caregiverNotification);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error distributing notification to caregivers: {ex.Message}");
            }
        }
    }
}