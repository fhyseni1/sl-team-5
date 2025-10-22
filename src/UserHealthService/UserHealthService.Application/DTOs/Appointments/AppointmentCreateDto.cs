using System;
namespace UserHealthService.Application.DTOs.Appointments
{
    public class AppointmentCreateDto
    {
        public Guid UserId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;  
        public string EndTime { get; set; } = string.Empty;
        public string? Purpose { get; set; }
        public string? Notes { get; set; }
        public string? PhoneNumber { get; set; }
    }
}