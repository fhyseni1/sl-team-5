using System;

namespace UserHealthService.Application.DTOs.Users
{
    public class UserCreateDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public UserType Type { get; set; }
    }
}