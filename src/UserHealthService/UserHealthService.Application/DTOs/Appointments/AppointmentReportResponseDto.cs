namespace UserHealthService.Application.DTOs.Appointments
{
    public class AppointmentReportResponseDto
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

