using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Relationships;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Mappings
{
    public class UserRelationshipProfile : Profile
    {
        public UserRelationshipProfile()
        {
            CreateMap<UserRelationship, UserRelationshipResponseDto>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : string.Empty))
                    .ForMember(dest => dest.RelatedUserName, opt => opt.MapFrom(src => src.RelatedUser != null ? src.RelatedUser.FirstName : string.Empty));

            CreateMap<UserRelationshipCreateDto, UserRelationship>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UserRelationshipUpdateDto, UserRelationship>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}