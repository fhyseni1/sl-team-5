// UserHealthService.Application/DTOs/Users/DoctorPatientDtos.cs
namespace UserHealthService.Application.DTOs.Users
{
    public class DoctorPatientDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Specialty { get; set; }
           public bool IsActive { get; set; } 
    }

    public class PatientDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? LastAppointment { get; set; }
        public int TotalAppointments { get; set; }
         public bool IsActive { get; set; }
    }
}