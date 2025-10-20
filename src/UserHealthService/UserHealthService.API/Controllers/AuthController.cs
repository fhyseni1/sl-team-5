using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Auth;
using UserHealthService.Application.Interfaces;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var tokens = await _authService.RegisterAsync(dto, ct);
            SetAuthCookies(tokens);
            return Ok(new { message = "Registration successful", tokens });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            var tokens = await _authService.LoginAsync(dto, ct);
            SetAuthCookies(tokens);
            return Ok(new { message = "Login successful", tokens });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var tokens = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
            SetAuthCookies(tokens);
            return Ok(new { message = "Token refreshed", tokens });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            await _authService.LogoutAsync(request.RefreshToken, ct);
            ClearAuthCookies();
            return Ok(new { message = "Logout successful" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
        {
            var user = await _authService.GetCurrentUserAsync(ct);
            return Ok(user);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
        {
            await _authService.ChangePasswordAsync(dto, ct);
            return Ok(new { message = "Password changed successfully" });
        }

        private void SetAuthCookies(TokenResponseDto tokens)
        {
            Response.Cookies.Append("access_token", tokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refresh_token", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
        }
    }

    public record RefreshTokenRequest(string RefreshToken);
}