using System;
using UserHealthService.Domain.Enums;
namespace UserHealthService.Application.DTOs.Users
{
    public class UserUpdateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}