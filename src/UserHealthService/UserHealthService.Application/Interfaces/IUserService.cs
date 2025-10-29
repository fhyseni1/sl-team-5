using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.DTOs.UserProfiles;

namespace UserHealthService.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserResponseDto>> GetActiveUsersAsync();
        Task<UserResponseDto> CreateUserAsync(UserCreateDto userCreateDto);
        Task<UserResponseDto> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);
        Task<UserResponseDto> UpdateUserProfileAsync(Guid userId, UserProfileUpdateDto profileUpdateDto);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
         Task<List<DoctorPatientDto>> GetDoctorsAsync();
    Task<List<PatientDto>> GetDoctorPatientsAsync(Guid doctorId);
    Task<List<PatientDto>> GetAllPatientsAsync();
        Task<int> GetActiveUsersCountAsync(CancellationToken ct = default);
        Task<int> GetAllUsersCountAsync(CancellationToken ct = default);
        Task<UserDashboardDto> GetUserDashboardAsync(Guid userId);
    }
}