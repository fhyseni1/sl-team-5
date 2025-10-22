using UserHealthService.Domain.Enums;

namespace UserHealthService.Domain.Entities
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
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
        public bool ReminderSent { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual User User { get; set; } = null!;
    }
}

