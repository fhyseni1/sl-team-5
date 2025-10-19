using AutoMapper;
using MedicationService.Application.DTOs.Schedules;
using MedicationService.Domain.Entities;
using System;

namespace MedicationService.Application.Mappings
{
    public class MedicationScheduleProfile : Profile
    {
        public MedicationScheduleProfile()
        {

            CreateMap<MedicationSchedule, ScheduleResponseDto>()
          .ForMember(dest => dest.MedicationName, opt => opt.MapFrom(src => src.Medication.Name));

            CreateMap<ScheduleCreateDto, MedicationSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Medication, opt => opt.Ignore())
                .ForMember(dest => dest.Reminders, opt => opt.Ignore());

            CreateMap<ScheduleUpdateDto, MedicationSchedule>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Medication, opt => opt.Ignore())
                .ForMember(dest => dest.Reminders, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MedicationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());


        }
    }
}
