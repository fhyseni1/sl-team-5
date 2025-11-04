using AutoMapper;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Mappings
{
    public class AppointmentReportProfile : Profile
    {
        public AppointmentReportProfile()
        {
            // Create -> Entity
            CreateMap<AppointmentReportCreateDto, AppointmentReport>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())
                .ForMember(dest => dest.ReportDate, opt => opt.Ignore()); // ADD THIS LINE

            // Update -> Entity
            CreateMap<AppointmentReportUpdateDto, AppointmentReport>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
                .ForMember(dest => dest.ReportDate, opt => opt.Ignore()) // Already good
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore());

            // Entity -> Response
            CreateMap<AppointmentReport, AppointmentReportResponseDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src =>
                    src.Doctor != null ? src.Doctor.Name : string.Empty))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src =>
                    src.Doctor != null ? src.Doctor.Specialty : string.Empty));
        }
    }
}