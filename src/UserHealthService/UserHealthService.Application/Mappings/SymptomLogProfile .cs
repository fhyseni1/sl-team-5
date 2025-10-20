using AutoMapper;
using UserHealthService.Application.DTOs.Symptoms;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Mappings
{
    public class SymptomLogProfile : Profile
    {
        public SymptomLogProfile()
        {
            CreateMap<SymptomLogCreateDto, SymptomLog>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<SymptomLogUpdateDto, SymptomLog>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<SymptomLog, SymptomLogResponseDto>();
        }
    }
}