using UserHealthService.Application.DTOs.Notifications;

namespace UserHealthService.Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDto>> GetAllAsync();
        Task<NotificationResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<NotificationResponseDto>> GetUnreadByUserIdAsync(Guid userId);
        Task<NotificationResponseDto> CreateAsync(NotificationCreateDto dto);
        Task<bool> MarkAsReadAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
    }
}
