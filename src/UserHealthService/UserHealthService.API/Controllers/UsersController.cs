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
        private readonly IAuthService _authService;

        public UsersController(
            IUserService userService,
            IUserRepository userRepository, 
            UserHealthDbContext context, 
            ILogger<UsersController> logger, 
            IAuthService authService)
        {
            _userService = userService;
            _userRepository = userRepository; 
            _context = context;
            _logger = logger;
            _authService = authService;
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
        public async Task<ActionResult<List<DoctorPatientDto>>> GetAvailableDoctors()
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
        public async Task<ActionResult<List<DoctorPatientDto>>> GetDoctors()
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
        [Authorize(Roles = "ClinicAdmin")] 
        public async Task<IActionResult> AssignAssistantToDoctor([FromBody] AssignAssistantDto dto)
        {
            try
            {
                Console.WriteLine($"🟡 === STARTING ASSIGNMENT ===");
                Console.WriteLine($"🟡 Received - DoctorId: {dto.DoctorId}, AssistantId: {dto.AssistantId}");

                // Get current user
                var currentUser = await _authService.GetCurrentUserAsync();
                Console.WriteLine($"🟡 Current user: {currentUser.Id} ({currentUser.Email}), Type: {currentUser.Type}");

                if (currentUser.Type != UserType.ClinicAdmin)
                {
                    return Forbid("Only clinic admins can assign assistants");
                }

                var clinic = await _context.Clinics
                    .FirstOrDefaultAsync(c => c.AdminUserId == currentUser.Id);
                    
                if (clinic == null)
                {
                    Console.WriteLine($"❌ Clinic not found for admin {currentUser.Id}");
                    return StatusCode(403, "Clinic not found for this admin");
                }

                Console.WriteLine($"🟡 Found clinic: {clinic.ClinicName} (ID: {clinic.Id})");

                var doctorUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.DoctorId && u.Type == UserType.HealthcareProvider);
                    
                if (doctorUser == null)
                {
                    Console.WriteLine($"❌ Doctor user not found: {dto.DoctorId}");
                    return BadRequest("Doctor not found");
                }

                var doctorInClinic = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.ClinicId == clinic.Id && 
                             (d.Name.Contains(doctorUser.FirstName) || 
                              d.Name.Contains(doctorUser.LastName) ||
                              doctorUser.Email.Contains(d.Name.ToLower().Replace(" ", "."))));
                
                if (doctorInClinic == null)
                {
                    Console.WriteLine($"❌ DOCTOR VALIDATION FAILED:");
                    Console.WriteLine($"❌ Doctor {dto.DoctorId} not found in clinic {clinic.Id}");
               
                    var clinicDoctors = await _context.Doctors
                        .Where(d => d.ClinicId == clinic.Id)
                        .Select(d => new { d.Id, d.Name })
                        .ToListAsync();
                        
                    Console.WriteLine($"🟡 Available doctors in clinic {clinic.Id}:");
                    foreach (var doc in clinicDoctors)
                    {
                        Console.WriteLine($"   - {doc.Id}: {doc.Name}");
                    }
                    
                    return StatusCode(403, "You can only assign assistants to doctors in your clinic");
                }

                Console.WriteLine($"✅ Doctor validation passed: {doctorInClinic.Name}");

                var assistantUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.AssistantId && u.Type == UserType.Assistant);
                    
                if (assistantUser == null)
                {
                    Console.WriteLine($"❌ Assistant user not found: {dto.AssistantId}");
                    return BadRequest("Assistant not found");
                }

                Console.WriteLine($"✅ Assistant user found: {assistantUser.FirstName} {assistantUser.LastName}");

                var existingAssignment = await _context.DoctorAssistants
                    .FirstOrDefaultAsync(da => da.DoctorId == dto.DoctorId && da.AssistantId == dto.AssistantId && da.IsActive);
                    
                if (existingAssignment != null)
                {
                    Console.WriteLine("❌ Assignment already exists");
                    return BadRequest("This assistant is already assigned to this doctor");
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

                Console.WriteLine($"✅ ASSIGNMENT SUCCESSFUL: {assignment.Id}");
                Console.WriteLine($"🟡 === ASSIGNMENT COMPLETE ===");

                return Ok(new { 
                    message = "Assistant assigned to doctor successfully",
                    assignmentId = assignment.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ === ASSIGNMENT ERROR ===");
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return StatusCode(500, "Error assigning assistant to doctor");
            }
        }

        [HttpPost("assign-clinic-assistant")]
        [Authorize(Roles = "ClinicAdmin")]
        public async Task<IActionResult> AssignClinicAssistant([FromBody] AssignAssistantDto dto)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                
                Console.WriteLine($"🟡 ClinicAdmin {currentUser.Id} trying to assign assistant {dto.AssistantId} to doctor {dto.DoctorId}");

                var clinic = await _context.Clinics
                    .FirstOrDefaultAsync(c => c.AdminUserId == currentUser.Id);
                    
                if (clinic == null)
                {
                    Console.WriteLine("❌ Clinic not found for admin");
                    return StatusCode(403, "Clinic not found for this admin");
                }

                Console.WriteLine($"🟡 Found clinic: {clinic.ClinicName} (ID: {clinic.Id})");

                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.ClinicId == clinic.Id);
                    
                if (doctor == null)
                {
                    Console.WriteLine($"❌ Doctor {dto.DoctorId} not found in clinic {clinic.Id}");
                    return StatusCode(403, "You can only assign assistants to doctors in your clinic");
                }

                Console.WriteLine($"🟡 Found doctor: {doctor.Name} in clinic");

                var doctorUser = await _userRepository.GetByIdAsync(dto.DoctorId);
                if (doctorUser == null || doctorUser.Type != UserType.HealthcareProvider)
                {
                    return BadRequest("Doctor not found");
                }

                var assistant = await _userRepository.GetByIdAsync(dto.AssistantId);
                if (assistant == null || assistant.Type != UserType.Assistant)
                {
                    return BadRequest("Assistant not found");
                }

                Console.WriteLine($"🟡 Both doctor and assistant users found");

                var existingAssignment = await _context.DoctorAssistants
                    .FirstOrDefaultAsync(da => da.DoctorId == dto.DoctorId && da.AssistantId == dto.AssistantId && da.IsActive);
                    
                if (existingAssignment != null)
                {
                    return BadRequest("This assistant is already assigned to this doctor");
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

                Console.WriteLine($"✅ Assistant assigned successfully");

                return Ok(new { message = "Assistant assigned to doctor successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AssignClinicAssistant: {ex.Message}");
                _logger.LogError(ex, "Error assigning assistant to doctor");
                return StatusCode(500, "Error assigning assistant to doctor");
            }
        }

        [HttpGet("assistant/{assistantId}/doctors")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<List<DoctorPatientDto>>> GetAssistantDoctors(Guid assistantId)
        {
            try
            {
              
                var doctorAssignments = await _context.DoctorAssistants
                    .Where(da => da.AssistantId == assistantId && da.IsActive)
                    .Include(da => da.Doctor)
                    .ToListAsync();

                var doctors = doctorAssignments.Select(da => new DoctorPatientDto
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
        [Authorize(Roles = "ClinicAdmin")] 
        public async Task<ActionResult<List<UserResponseDto>>> GetAllAssistants()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
               
                var clinic = await _context.Clinics
                    .FirstOrDefaultAsync(c => c.AdminUserId == currentUser.Id);
                    
                if (clinic == null)
                {
                    return BadRequest("Clinic admin does not have a clinic assigned");
                }

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

        [HttpGet("clinics/{clinicId}/assistants")]
        [Authorize(Roles = "ClinicAdmin")]
        public async Task<ActionResult<List<UserResponseDto>>> GetClinicAssistants(Guid clinicId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
         
                var clinic = await _context.Clinics
                    .FirstOrDefaultAsync(c => c.Id == clinicId && c.AdminUserId == currentUser.Id);
                    
                if (clinic == null)
                {
                    return Forbid("You can only view assistants from your own clinic");
                }

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
                _logger.LogError(ex, "Error retrieving clinic assistants");
                return StatusCode(500, "Error retrieving clinic assistants");
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
        [Authorize(Roles = "Admin,ClinicAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var userToDelete = await _userRepository.GetByIdAsync(id);
                
                if (userToDelete == null)
                {
                    return NotFound($"User with ID '{id}' not found");
                }
                
                if (currentUser.Type == UserType.ClinicAdmin && userToDelete.Type != UserType.Assistant)
                {
                    return Forbid("Clinic admins can only delete assistants");
                }
                
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
    }
}