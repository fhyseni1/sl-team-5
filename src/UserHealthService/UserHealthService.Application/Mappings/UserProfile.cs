using AutoMapper;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.DTOs.UserProfiles;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // User mappings
            CreateMap<User, UserResponseDto>();
            CreateMap<UserCreateDto, User>();
            CreateMap<UserUpdateDto, User>();

            // UserProfile mappings
            CreateMap<Domain.Entities.UserProfile, UserProfileResponseDto>();
            CreateMap<UserProfileCreateDto, Domain.Entities.UserProfile>();
            CreateMap<UserProfileUpdateDto, Domain.Entities.UserProfile>();
        }
    }
}

