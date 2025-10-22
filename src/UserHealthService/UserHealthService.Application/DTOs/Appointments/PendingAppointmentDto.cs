namespace UserHealthService.Application.DTOs.Appointments
{
    public class PendingAppointmentDto
    {
        public Guid Id { get; set; }
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}