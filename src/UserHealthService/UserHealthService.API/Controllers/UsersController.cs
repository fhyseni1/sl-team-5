// UserHealthService.API/Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.DTOs.UserProfiles;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using UserHealthService.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly UserHealthDbContext _context; 
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IUserRepository userRepository, 
            UserHealthDbContext context, 
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _userRepository = userRepository; 
            _context = context; 
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        [HttpGet("available-doctors")]
        [Authorize]
        public async Task<ActionResult<List<DoctorDto>>> GetAvailableDoctors()
        {
            try
            {
                var doctors = await _userService.GetDoctorsAsync();
                return Ok(doctors.Where(d => d.IsActive).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available doctors");
                return StatusCode(500, "An error occurred while retrieving doctors");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID '{id}' not found");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        [HttpGet("{userId}/dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDashboardDto>> GetUserDashboard(Guid userId)
        {
            try
            {
                var dashboard = await _userService.GetUserDashboardAsync(userId);
                return Ok(dashboard);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User with ID {UserId} not found for dashboard", userId);
                return NotFound($"User with ID '{userId}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard for user with ID {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving the user dashboard");
            }
        }

        [HttpGet("doctors")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<DoctorDto>>> GetDoctors()
        {
            try
            {
                var doctors = await _userService.GetDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors");
                return StatusCode(500, "An error occurred while retrieving doctors");
            }
        }

        [HttpPost("assign-assistant")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignAssistantToDoctor([FromBody] AssignAssistantDto dto)
        {
            try
            {
         
                var doctor = await _userRepository.GetByIdAsync(dto.DoctorId);
                if (doctor == null || doctor.Type != UserType.HealthcareProvider)
                {
                    return BadRequest("Doctor not found");
                }

                var assistant = await _userRepository.GetByIdAsync(dto.AssistantId);
                if (assistant == null || assistant.Type != UserType.Assistant)
                {
                    return BadRequest("Assistant not found");
                }

                var assignment = new DoctorAssistant
                {
                    Id = Guid.NewGuid(),
                    DoctorId = dto.DoctorId,
                    AssistantId = dto.AssistantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DoctorAssistants.Add(assignment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Assistant assigned to doctor successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning assistant to doctor");
                return StatusCode(500, "Error assigning assistant to doctor");
            }
        }

        [HttpGet("assistant/{assistantId}/doctors")]
        [Authorize(Roles = "Admin,Assistant")]
        public async Task<ActionResult<List<DoctorDto>>> GetAssistantDoctors(Guid assistantId)
        {
            try
            {
                // Tani përdor tabelën e vërtetë
                var doctorAssignments = await _context.DoctorAssistants
                    .Where(da => da.AssistantId == assistantId && da.IsActive)
                    .Include(da => da.Doctor)
                    .ToListAsync();

                var doctors = doctorAssignments.Select(da => new DoctorDto
                {
                    Id = da.Doctor.Id,
                    Name = $"{da.Doctor.FirstName} {da.Doctor.LastName}",
                    Email = da.Doctor.Email,
                    PhoneNumber = da.Doctor.PhoneNumber,
                    Specialty = "General Practitioner",
                    IsActive = da.Doctor.IsActive
                }).ToList();

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assistant doctors");
                return StatusCode(500, "Error retrieving doctors");
            }
        }
[HttpGet("assistants")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<List<UserResponseDto>>> GetAllAssistants()
{
    try
    {
        var assistants = await _context.Users
            .Where(u => u.Type == UserType.Assistant && u.IsActive)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                Type = u.Type,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(assistants);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving assistants");
        return StatusCode(500, "Error retrieving assistants");
    }
}

        [HttpGet("doctor/{doctorId}/patients")]
        [Authorize]
        public async Task<ActionResult<List<PatientDto>>> GetDoctorPatients(Guid doctorId)
        {
            try
            {
                Console.WriteLine($"🔍 GetDoctorPatients called with doctorId: {doctorId}");
                
                var patients = await _userService.GetDoctorPatientsAsync(doctorId);
                
                Console.WriteLine($"✅ Returning {patients.Count} patients for doctor {doctorId}");
                return Ok(patients);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Doctor with ID {DoctorId} not found", doctorId);
                return NotFound($"Doctor with ID '{doctorId}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients for doctor {DoctorId}", doctorId);
                Console.WriteLine($"❌ ERROR in GetDoctorPatients: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred while retrieving patients: {ex.Message}");
            }
        }

        [HttpGet("patients")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<PatientDto>>> GetAllPatients()
        {
            try
            {
                var patients = await _userService.GetAllPatientsAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return StatusCode(500, "An error occurred while retrieving patients");
            }
        }

        [HttpGet("email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound($"User with email '{email}' not found");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with email {Email}", email);
                return StatusCode(500, "An error occurred while retrieving the user");
            }
        }

        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users");
                return StatusCode(500, "An error occurred while retrieving active users");
            }
        }

        [HttpGet("stats/active-count")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<int>> GetActiveUsersCount(CancellationToken ct)
        {
            try
            {
                var count = await _userService.GetActiveUsersCountAsync(ct);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active users count");
                return StatusCode(500, "An error occurred while retrieving active users count");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.CreateUserAsync(userCreateDto);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating user");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, [FromBody] UserUpdateDto userUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.UpdateUserAsync(id, userUpdateDto);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User with ID {UserId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        [HttpPut("{id}/profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponseDto>> UpdateUserProfile(Guid id, [FromBody] UserProfileUpdateDto profileUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.UpdateUserProfileAsync(id, profileUpdateDto);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User with ID {UserId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user with ID {UserId}", id);
                return StatusCode(500, "An error occurred while updating the user profile");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound($"User with ID '{id}' not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        [HttpGet("count")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUsersCount(CancellationToken ct)
        {
            try
            {
                var count = await _userService.GetAllUsersCountAsync(ct);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users count");
                return StatusCode(500, "An error occurred while retrieving users count");
            }
        }

      
        public class DoctorDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string? Specialty { get; set; }
            public bool IsActive { get; set; } 
        }

        public class PatientDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public DateTime? LastAppointment { get; set; }
            public int TotalAppointments { get; set; }
        }

        public class AssignAssistantDto
        {
            public Guid DoctorId { get; set; }
            public Guid AssistantId { get; set; }
        }
    }
}