using System;
using System.ComponentModel.DataAnnotations;

namespace UserHealthService.Application.DTOs.Auth
{
    public record RegisterDoctorDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; init; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; init; }

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; init; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; init; }

        public string? PhoneNumber { get; init; }

        [Required(ErrorMessage = "Specialty is required")]
        public string Specialty { get; init; }

        [Required(ErrorMessage = "Clinic ID is required")]
        public Guid ClinicId { get; init; }
    }
}