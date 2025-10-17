using AutoMapper;
using MedicationService.Domain.DTOs.Interactions;
using MedicationService.Domain.Entities;

namespace MedicationService.Application.Mappings
{
    public class DrugInteractionProfile : Profile
    {
        public DrugInteractionProfile()
        {
            CreateMap<DrugInteraction, InteractionResponseDto>()
                .ForMember(dest => dest.MedicationName,
                    opt => opt.MapFrom(src => src.Medication != null ? src.Medication.Name : string.Empty));

            CreateMap<InteractionCreateDto, DrugInteraction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DetectedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsAcknowledged, opt => opt.Ignore())
                .ForMember(dest => dest.Medication, opt => opt.Ignore());

            CreateMap<InteractionUpdateDto, DrugInteraction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MedicationId, opt => opt.Ignore())
                .ForMember(dest => dest.InteractingDrugName, opt => opt.Ignore())
                .ForMember(dest => dest.DetectedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Medication, opt => opt.Ignore());
        }
    }
}

