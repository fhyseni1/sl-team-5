using AutoMapper;
using MedicationService.Application.DTOs.Doses;
using MedicationService.Domain.Entities;

namespace MedicationService.Application.Mappings
{
    public class MedicationDoseProfile : Profile
    {
        public MedicationDoseProfile()
        {
            CreateMap<MedicationDose, DoseResponseDto>()
                .ForMember(dest => dest.MedicationName,
                    opt => opt.MapFrom(src => src.Medication.Name));

            CreateMap<DoseCreateDto, MedicationDose>();
            CreateMap<DoseUpdateDto, MedicationDose>();
        }
    }
}
