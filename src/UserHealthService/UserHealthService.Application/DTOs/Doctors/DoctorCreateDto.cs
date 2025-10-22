namespace UserHealthService.Application.DTOs.Doctors
{
    public class DoctorCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}