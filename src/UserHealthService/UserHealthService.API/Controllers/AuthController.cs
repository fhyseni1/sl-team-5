using AutoMapper;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Auth;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.Interfaces;
using UserHealthService.Application.Services;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly UserHealthDbContext _context;

        public AuthController(
            IAuthService authService,
            IUserRepository userRepository,
            IMapper mapper,
            IUserService userService,
            UserHealthDbContext context)
        {
            _authService = authService;
            _userService = userService;
            _mapper = mapper;
            _userRepository = userRepository;
            _context = context;
        }

        [HttpPost("register-doctor")]
        [Authorize(Roles = "Admin,ClinicAdmin")]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto dto, CancellationToken ct)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Type != UserType.Admin && currentUser.Type != UserType.ClinicAdmin)
                {
                    return Forbid("Only administrators or clinic admins can register doctors");
                }
                // If ClinicAdmin, ensure clinicId matches their clinic
                if (currentUser.Type == UserType.ClinicAdmin)
                {
                    var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.AdminUserId == currentUser.Id, ct);
                    if (clinic == null || clinic.Id != dto.ClinicId)
                    {
                        return Forbid("Clinic admins can only register doctors for their own clinic");
                    }
                }
                if (string.IsNullOrEmpty(dto.Password))
                {
                    return BadRequest(new { message = "Password is required" });
                }
                var registerDto = new RegisterDto(
                    Email: dto.Email,
                    Password: dto.Password,
                    FirstName: dto.FirstName,
                    LastName: dto.LastName,
                    PhoneNumber: dto.PhoneNumber,
                    Type: UserType.HealthcareProvider,
                    Specialty: dto.Specialty,
                    ClinicName: null,
                    Address: null
                );
                var tokens = await _authService.RegisterAsync(registerDto, ct);
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Failed to create user" });
                }
                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    Name = $"{dto.FirstName} {dto.LastName}",
                    Specialty = dto.Specialty ?? "General",
                    ClinicId = dto.ClinicId,
                    PhoneNumber = dto.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync(ct);
             
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
                    doctorId = doctor.Id,
                    clinicId = doctor.ClinicId,
                    tokens
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register-clinic")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterClinic([FromBody] RegisterClinicDto dto, CancellationToken ct)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Type != UserType.Admin)
                {
                    return Forbid("Only administrators can register clinics");
                }
                if (string.IsNullOrEmpty(dto.Password))
                {
                    return BadRequest(new { message = "Password is required" });
                }
                var registerDto = new RegisterDto(
                    Email: dto.Email,
                    Password: dto.Password,
                    FirstName: dto.FirstName,
                    LastName: dto.LastName,
                    PhoneNumber: dto.PhoneNumber,
                    Type: UserType.ClinicAdmin,
                    Specialty: null,
                    ClinicName: dto.ClinicName,
                    Address: dto.Address
                );
                var tokens = await _authService.RegisterAsync(registerDto, ct);
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Failed to create user" });
                }
                var clinic = new Clinic
                {
                    Id = Guid.NewGuid(),
                    ClinicName = dto.ClinicName,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    AdminUserId = user.Id
                };
                _context.Clinics.Add(clinic);
                await _context.SaveChangesAsync(ct);
                
                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    type = user.Type.ToString(),
                    phoneNumber = user.PhoneNumber,
                    isActive = user.IsActive,
                    clinicId = clinic.Id,
                    clinicName = clinic.ClinicName,
                    tokens
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(_mapper.Map<UserResponseDto>(user));
        }
        [HttpPost("register-clinic-assistant")]
[Authorize(Roles = "ClinicAdmin")]
public async Task<IActionResult> RegisterClinicAssistant([FromBody] RegisterDto dto, CancellationToken ct)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        // Verify the clinic admin has a clinic
        var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.AdminUserId == currentUser.Id, ct);
        if (clinic == null)
        {
            return BadRequest(new { message = "Clinic not found for this admin" });
        }

        if (string.IsNullOrEmpty(dto.Password))
        {
            return BadRequest(new { message = "Password is required" });
        }
        
        // Set type to Assistant automatically
        var registerDto = dto with
        {
            Type = UserType.Assistant
        };
        
        var tokens = await _authService.RegisterAsync(registerDto, ct);
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        if (user == null)
        {
            return BadRequest(new { message = "Failed to create user" });
        }
       
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
            clinicId = clinic.Id,
            tokens
        });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
      
[HttpPost("register-assistant")]
[Authorize(Roles = "Admin,ClinicAdmin")] 
public async Task<IActionResult> RegisterAssistant([FromBody] RegisterDto dto, CancellationToken ct)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser.Type == UserType.ClinicAdmin)
        {
           
        }

        if (string.IsNullOrEmpty(dto.Password))
        {
            return BadRequest(new { message = "Password is required" });
        }
        
        var registerDto = dto with
        {
            Type = UserType.Assistant
        };
        
        var tokens = await _authService.RegisterAsync(registerDto, ct);
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        if (user == null)
        {
            return BadRequest(new { message = "Failed to create user" });
        }
        
       
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

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            try
            {
                var tokens = await _authService.RegisterAsync(dto, ct);
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Failed to create user" });
                }
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

        [HttpGet("clinics/admin/{adminUserId}")]
        [Authorize(Roles = "ClinicAdmin")]
        public async Task<IActionResult> GetClinicByAdmin(Guid adminUserId, CancellationToken ct)
        {
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.AdminUserId == adminUserId, ct);
            if (clinic == null)
            {
                return NotFound(new { message = "Clinic not found for this admin" });
            }
            return Ok(new
            {
                id = clinic.Id,
                clinicName = clinic.ClinicName,
                phoneNumber = clinic.PhoneNumber,
                address = clinic.Address,
                createdAt = clinic.CreatedAt,
                isActive = clinic.IsActive
            });
        }

        [HttpGet("doctors/clinic/{clinicId}")]
        [Authorize(Roles = "ClinicAdmin")]
        public async Task<IActionResult> GetDoctorsByClinic(Guid clinicId, CancellationToken ct)
        {
            var doctors = await _context.Doctors
                .Where(d => d.ClinicId == clinicId && d.IsActive)
                .Join(
                    _context.Users,
                    d => d.Name,
                    u => $"{u.FirstName} {u.LastName}",
                    (d, u) => new
                    {
                        id = u.Id,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        specialty = d.Specialty,
                        phoneNumber = d.PhoneNumber
                    }
                )
                .ToListAsync(ct);
            return Ok(doctors);
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
                Secure = false,
                SameSite = SameSiteMode.Lax, 
                Expires = DateTime.UtcNow.AddMinutes(15)
            };
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Lax, 
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Console.WriteLine($"Setting access_token: {tokens.AccessToken.Substring(0, 20)}...");
            Console.WriteLine($"Setting refresh_token: {tokens.RefreshToken.Substring(0, 20)}...");
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