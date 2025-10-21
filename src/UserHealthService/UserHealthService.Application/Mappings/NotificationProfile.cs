using AutoMapper;
using UserHealthService.Application.DTOs.Notifications;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Mappings
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationResponseDto>();
            CreateMap<NotificationCreateDto, Notification>();
            CreateMap<NotificationUpdateDto, Notification>();
        }
    }
}
