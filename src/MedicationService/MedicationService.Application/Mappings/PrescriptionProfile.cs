using AutoMapper;
using MedicationService.Domain.DTOs.Prescriptions;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.Mappings
{
    public class PrescriptionProfile : Profile
    {
        public PrescriptionProfile()
        {
            CreateMap<PrescriptionCreateDto, Prescription>()
               .ForMember(dest => dest.MedicationId, opt => opt.MapFrom(src => src.MedicationId))
               .ForMember(dest => dest.PrescriptionNumber, opt => opt.MapFrom(src => src.PrescriptionNumber))
               .ForMember(dest => dest.PrescriberName, opt => opt.MapFrom(src => src.PrescriberName))
               .ForMember(dest => dest.PrescriberContact, opt => opt.MapFrom(src => src.PrescriberContact))
               .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src => src.PharmacyName))
               .ForMember(dest => dest.PharmacyContact, opt => opt.MapFrom(src => src.PharmacyContact))
               .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate))
               .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
               .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PrescriptionStatus.Active))
               .ForMember(dest => dest.RemainingRefills, opt => opt.MapFrom(src => 0))
               .ForMember(dest => dest.Medication, opt => opt.Ignore());


            CreateMap<PrescriptionUpdateDto, Prescription>()
                .ForMember(dest => dest.PrescriptionNumber,
                           opt => opt.Condition(src => !string.IsNullOrEmpty(src.PrescriptionNumber)))
                .ForMember(dest => dest.PrescriberName,
                           opt => opt.Condition(src => !string.IsNullOrEmpty(src.PrescriberName)))
                .ForMember(dest => dest.PrescriberContact,
                           opt => opt.Condition(src => !string.IsNullOrEmpty(src.PrescriberContact)))
                .ForMember(dest => dest.PharmacyName,
                           opt => opt.Condition(src => !string.IsNullOrEmpty(src.PharmacyName)))
                .ForMember(dest => dest.PharmacyContact,
                           opt => opt.Condition(src => !string.IsNullOrEmpty(src.PharmacyContact)))
                .ForMember(dest => dest.ExpiryDate, opt => opt.Condition(src => src.ExpiryDate.HasValue))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Notes,
                           opt => opt.Condition(src => !string.IsNullOrEmpty(src.Notes)));
           

        
            CreateMap<Prescription, PrescriptionResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MedicationId, opt => opt.MapFrom(src => src.MedicationId))
                .ForMember(dest => dest.MedicationName, opt => opt.MapFrom(src => src.Medication.Name))
                .ForMember(dest => dest.PrescriptionNumber, opt => opt.MapFrom(src => src.PrescriptionNumber))
                .ForMember(dest => dest.PrescriberName, opt => opt.MapFrom(src => src.PrescriberName))
                .ForMember(dest => dest.PrescriberContact, opt => opt.MapFrom(src => src.PrescriberContact))
                .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src => src.PharmacyName))
                .ForMember(dest => dest.PharmacyContact, opt => opt.MapFrom(src => src.PharmacyContact))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsExpired,
                           opt => opt.MapFrom(src => src.ExpiryDate.HasValue && src.ExpiryDate.Value < DateTime.UtcNow))
                .ForMember(dest => dest.DaysUntilExpiry,
                           opt => opt.MapFrom(src => src.ExpiryDate.HasValue ?
                               (int)(src.ExpiryDate.Value - DateTime.UtcNow).TotalDays : -1));
        }
    }
}
