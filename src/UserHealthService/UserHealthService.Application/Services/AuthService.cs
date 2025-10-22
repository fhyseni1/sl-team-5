using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using UserHealthService.Application.DTOs.Auth;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Application.Options;

namespace UserHealthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtOptions _jwtOptions;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IHttpContextAccessor httpContextAccessor,
            IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _httpContextAccessor = httpContextAccessor;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<TokenResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        {
            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Type = dto.Type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var tokens = GenerateTokens(user);
            await _refreshTokenRepository.AddAsync(tokens.refreshToken, user.Id, ct);

            return new TokenResponseDto(tokens.accessToken, tokens.refreshToken, tokens.expiresIn);
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is disabled");

            await _refreshTokenRepository.RemoveByUserIdAsync(user.Id, ct);

            var tokens = GenerateTokens(user);
            await _refreshTokenRepository.AddAsync(tokens.refreshToken, user.Id, ct);

            return new TokenResponseDto(tokens.accessToken, tokens.refreshToken, tokens.expiresIn);
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, ct)
                ?? throw new UnauthorizedAccessException("Invalid refresh token");

            if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token expired");

            var user = await _userRepository.GetByIdAsync(storedToken.UserId)
                ?? throw new UnauthorizedAccessException("User not found");

            storedToken.IsRevoked = true;

            var tokens = GenerateTokens(user);
            await _refreshTokenRepository.AddAsync(tokens.refreshToken, user.Id, ct);

            return new TokenResponseDto(tokens.accessToken, tokens.refreshToken, tokens.expiresIn);
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, ct);
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
            }
        }

        public async Task<UserResponseDto> GetCurrentUserAsync(CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found");

            Console.WriteLine($"🔍 getMe() - DB Type: {user.Type} ({(int)user.Type})"); 

            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Type = user.Type,  
                IsActive = user.IsActive,
                DateOfBirth = user.DateOfBirth,  
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid old password");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            return true;
        }

        private (string accessToken, string refreshToken, int expiresIn) GenerateTokens(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("type", ((int)user.Type).ToString()),
                new Claim(ClaimTypes.Role, user.Type.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(_jwtOptions.AccessTokenLifetime),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expiresIn = (int)_jwtOptions.AccessTokenLifetime.TotalSeconds;

            return (accessToken, refreshToken, expiresIn);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found"));
        }
    }
}
