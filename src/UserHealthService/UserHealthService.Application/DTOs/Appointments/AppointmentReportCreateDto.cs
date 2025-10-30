namespace UserHealthService.Application.DTOs.Appointments
{
    public class AppointmentReportCreateDto
    {
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
    }
}

