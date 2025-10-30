using System;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.DTOs.Appointments
{
    public class AppointmentResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DoctorId { get; set; } 
        public string UserName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
          public string? RejectionReason { get; set; } 
        public AppointmentStatus Status { get; set; }
        public string? Purpose { get; set; }
        public string? Notes { get; set; }
        public string? PhoneNumber { get; set; }
        public bool ReminderSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime FullStartDateTime => AppointmentDate.Add(StartTime);
        public DateTime FullEndDateTime => AppointmentDate.Add(EndTime);
        public bool IsUpcoming => FullStartDateTime > DateTime.UtcNow;
        public bool IsPast => FullEndDateTime < DateTime.UtcNow;
    }
}