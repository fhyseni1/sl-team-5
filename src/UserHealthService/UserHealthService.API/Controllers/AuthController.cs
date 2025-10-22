using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Auth;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using BCrypt.Net;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public AuthController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            try
            {
                var tokens = await _authService.RegisterAsync(dto, ct);
                var user = await _userRepository.GetByEmailAsync(dto.Email);

                SetAuthCookies(tokens);

                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    type = user.Type.ToString(),
                    phoneNumber = user.PhoneNumber,
                    isActive = user.IsActive,
                    fullName = $"{user.FirstName} {user.LastName}",
                    tokens
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Invalid credentials" });

                var tokens = await _authService.LoginAsync(dto, ct);

                SetAuthCookies(tokens);

                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    type = user.Type.ToString(),
                    phoneNumber = user.PhoneNumber,
                    isActive = user.IsActive,
                    fullName = $"{user.FirstName} {user.LastName}",
                    tokens
                });
            }
            catch
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token provided");

            try
            {
                var tokens = await _authService.RefreshTokenAsync(refreshToken);
                if (tokens == null)
                    return Unauthorized("Invalid refresh token");

                SetAuthCookies(tokens);
                return Ok(tokens);
            }
            catch (UnauthorizedAccessException)
            {
                ClearAuthCookies();
                return Unauthorized("Refresh token expired or invalid");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken, ct);
            }

            ClearAuthCookies();
            return Ok(new { message = "Logout successful" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _authService.GetCurrentUserAsync();
            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                type = (int)user.Type,  
                phoneNumber = user.PhoneNumber,
                isActive = user.IsActive,
                fullName = $"{user.FirstName} {user.LastName}"
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
        {
            await _authService.ChangePasswordAsync(dto, ct);
            return Ok(new { message = "Password changed successfully" });
        }

        #region Cookie Helpers
        private void SetAuthCookies(TokenResponseDto tokens)
        {
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("access_token", tokens.AccessToken, accessCookieOptions);
            Response.Cookies.Append("refresh_token", tokens.RefreshToken, refreshCookieOptions);
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
        }
        #endregion
    }
}
