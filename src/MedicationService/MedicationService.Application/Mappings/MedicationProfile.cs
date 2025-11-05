using AutoMapper;
using MedicationService.Application.DTOs.Medications;
using MedicationService.Domain.Entities;

namespace MedicationService.Application.Mappings
{
    public class MedicationProfile : Profile
    {
        public MedicationProfile()
        {
            CreateMap<Medication, MedicationResponseDto>()
               
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.DoctorId))
                .ForMember(dest => dest.PrescribedBy, opt => opt.MapFrom(src => src.PrescribedBy))
                .ForMember(dest => dest.ActiveSchedulesCount,
                    opt => opt.MapFrom(src => src.Schedules.Count(s => s.IsActive)))
                .ForMember(dest => dest.UpcomingDosesCount,
                    opt => opt.MapFrom(src => src.Doses.Count(d => !d.IsTaken && d.ScheduledTime > DateTime.UtcNow)));

            CreateMap<MedicationCreateDto, Medication>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ScannedImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Schedules, opt => opt.Ignore())
                .ForMember(dest => dest.Doses, opt => opt.Ignore())
                .ForMember(dest => dest.DrugInteractions, opt => opt.Ignore())
                .ForMember(dest => dest.Prescriptions, opt => opt.Ignore())
               
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.DoctorId))
                .ForMember(dest => dest.PrescribedBy, opt => opt.MapFrom(src => src.PrescribedBy));

            CreateMap<MedicationUpdateDto, Medication>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Schedules, opt => opt.Ignore())
                .ForMember(dest => dest.Doses, opt => opt.Ignore())
                .ForMember(dest => dest.DrugInteractions, opt => opt.Ignore())
                .ForMember(dest => dest.Prescriptions, opt => opt.Ignore());
                
        }
    }
}