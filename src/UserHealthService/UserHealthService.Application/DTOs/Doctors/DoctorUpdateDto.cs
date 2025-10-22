namespace UserHealthService.Application.DTOs.Doctors
{
    public class DoctorUpdateDto
    {
        public string? Name { get; set; }
        public string? Specialty { get; set; }
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
    }
}