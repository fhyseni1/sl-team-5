using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.DTOs.UserProfiles;

namespace UserHealthService.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(Guid id);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
        Task<UserResponseDto> CreateUserAsync(UserCreateDto userCreateDto);
        Task<UserResponseDto> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);
        Task<UserResponseDto> UpdateUserProfileAsync(Guid userId, UserProfileUpdateDto profileUpdateDto);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<UserResponseDto>> GetActiveUsersAsync();
    }
}

