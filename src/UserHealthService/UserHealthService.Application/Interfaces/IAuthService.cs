using UserHealthService.Application.DTOs.Auth;
using UserHealthService.Application.DTOs.Users;

namespace UserHealthService.Application.Interfaces
{
    public interface IAuthService
    {

        Task<TokenResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
        Task<TokenResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
        Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task LogoutAsync(string refreshToken, CancellationToken ct = default);
        Task<UserResponseDto> GetCurrentUserAsync(CancellationToken ct = default);
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default);
    }
}