using AutoMapper;
using UserHealthService.Application.DTOs.Allergies;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Mappings
{
    public class AllergyProfile : Profile
    {
        public AllergyProfile()
        {
            CreateMap<AllergyCreateDto, Allergy>();
            CreateMap<AllergyUpdateDto, Allergy>();
            CreateMap<Allergy, AllergyResponseDto>();
            
            CreateMap<AllergyUpdateDto, Allergy>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}