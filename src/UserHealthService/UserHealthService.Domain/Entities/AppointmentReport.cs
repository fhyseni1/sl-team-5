namespace UserHealthService.Domain.Entities
{
    public class AppointmentReport
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime ReportDate { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Appointment Appointment { get; set; } = null!;
        public User User { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}

