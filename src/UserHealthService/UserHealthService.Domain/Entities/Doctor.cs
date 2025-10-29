// UserHealthService.Domain/Entities/Doctor.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserHealthService.Domain.Entities
{
    [Table("Doctors")]
    public class Doctor
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Specialty { get; set; }
        public string? ClinicName { get; set; }
        public Guid? ClinicId { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}