using AutoMapper;
using MedicationService.Application.DTOs.Reminders;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using System;

namespace MedicationService.Application.Mappings
{
    public class MedicationReminderProfile : Profile
    {
        public MedicationReminderProfile()
        {
            CreateMap<ReminderCreateDto, MedicationReminder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ReminderStatus.Scheduled))
                .ForMember(dest => dest.SnoozeCount, opt => opt.MapFrom(_ => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Medication, opt => opt.Ignore())
                .ForMember(dest => dest.Schedule, opt => opt.Ignore());


            CreateMap<ReminderUpdateDto, MedicationReminder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.MedicationId, opt => opt.Ignore())
                .ForMember(dest => dest.ScheduleId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.SentAt, opt => opt.Ignore())
                .ForMember(dest => dest.AcknowledgedAt, opt => opt.Ignore())
                .ForMember(dest => dest.SnoozeCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Medication, opt => opt.Ignore())
                .ForMember(dest => dest.Schedule, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                    srcMember != null &&
                    !(srcMember is DateTime dt && dt == default)));

            CreateMap<MedicationReminder, ReminderResponseDto>()
                .ForMember(dest => dest.MedicationName, opt => opt.MapFrom(src => src.Medication.Name));
        }
    }
}