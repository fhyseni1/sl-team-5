using System;

namespace UserHealthService.Application.DTOs.Appointments
{
    public class AppointmentUpdateDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Purpose { get; set; }
        public string? Notes { get; set; }
        public string? PhoneNumber { get; set; }
    }
}