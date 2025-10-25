using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Enums;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUserService _userService;
        private readonly ILogger<AppointmentsController> _logger;
         private readonly IAuthService _authService; 
        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentRepository appointmentRepository,
            IUserService userService,
            IAuthService authService, 
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _appointmentRepository = appointmentRepository;
            _userService = userService;
              _authService = authService; 
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAll()
        {
            var appointments = await _appointmentService.GetAllAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentResponseDto>> GetById(Guid id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetByUserId(Guid userId)
        {
            var appointments = await _appointmentService.GetByUserIdAsync(userId);
            return Ok(appointments);
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetUpcoming()
        {
            var appointments = await _appointmentService.GetUpcomingAsync();
            return Ok(appointments);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetByDateRange(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var appointments = await _appointmentService.GetAppointmentsByDateRangeAsync(fromDate, toDate);
            return Ok(appointments);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentResponseDto>> Create(AppointmentCreateDto createDto)
        {
            try
            {
                var createdAppointment = await _appointmentService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = createdAppointment.Id }, createdAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, "Error creating appointment");
            }
        }
        

        [HttpPut("{id}")]
        public async Task<ActionResult<AppointmentResponseDto>> Update(Guid id, AppointmentUpdateDto updateDto)
        {
            var updatedAppointment = await _appointmentService.UpdateAsync(id, updateDto);
            if (updatedAppointment == null) return NotFound();
            return Ok(updatedAppointment);
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> Cancel(Guid id)
        {
            var result = await _appointmentService.CancelAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var result = await _appointmentService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<PendingAppointmentDto>>> GetPending()
        {
            try
            {
                var appointments = await _appointmentRepository.GetPendingAppointmentsAsync();

                var pendingDtos = new List<PendingAppointmentDto>();
                foreach (var app in appointments)
                {
                    var user = await _userService.GetUserByIdAsync(app.UserId);
                    pendingDtos.Add(new PendingAppointmentDto
                    {
                        Id = app.Id,
                        UserFirstName = user?.FirstName ?? "Unknown",
                        UserLastName = user?.LastName ?? "",
                        DoctorName = app.DoctorName,
                        DoctorSpecialty = app.Specialty,
                        AppointmentDate = app.AppointmentDate,
                        StartTime = app.StartTime.ToString(@"hh\:mm"),
                        EndTime = app.EndTime.ToString(@"hh\:mm"),
                        Purpose = app.Purpose ?? "",
                        Notes = app.Notes
                    });
                }

                return Ok(pendingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending appointments");
                return StatusCode(500, "Error retrieving pending appointments");
            }
        }
       
[HttpGet("approved")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<List<AppointmentResponseDto>>> GetApprovedAppointments()
{
    try
    {
        var appointments = await _appointmentRepository.GetApprovedAppointmentsAsync();

        var approvedDtos = appointments.Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown User",
            DoctorName = a.DoctorName ?? string.Empty,
            Specialty = a.Specialty ?? string.Empty,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Purpose = a.Purpose ?? string.Empty,
            Status = a.Status
        }).ToList();

        return Ok(approvedDtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving approved appointments");
        return StatusCode(500, "Error retrieving approved appointments");
    }
}

[HttpGet("rejected")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<List<AppointmentResponseDto>>> GetRejectedAppointments()
{
    try
    {
        var appointments = await _appointmentRepository.GetRejectedAppointmentsAsync();

        var rejectedDtos = appointments.Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown User",
            DoctorName = a.DoctorName ?? string.Empty,
            Specialty = a.Specialty ?? string.Empty,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Purpose = a.Purpose ?? string.Empty,
            Status = a.Status,
            RejectionReason = a.RejectionReason 
        }).ToList();

        return Ok(rejectedDtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving rejected appointments");
        return StatusCode(500, "Error retrieving rejected appointments");
    }
}

        [HttpGet("user/{userId}/upcoming/count")]
        public async Task<ActionResult<int>> GetUpcomingCount(Guid userId)
        {
            try
            {
                var count = await _appointmentRepository.CountUpcomingByUserIdAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming count");
                return StatusCode(500, "Error retrieving count");
            }
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Approve(Guid id)
        {
            try
            {
                var result = await _appointmentRepository.ApproveAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving appointment {AppointmentId}", id);
                return StatusCode(500, "Error approving appointment");
            }
        }
    [HttpGet("assistant/pending")]
[Authorize(Roles = "Assistant")]
public async Task<ActionResult<List<AppointmentResponseDto>>> GetAssistantPendingAppointments()
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        var appointments = await _appointmentRepository.GetPendingAppointmentsForAssistantAsync(currentUser.Id);
        
        var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown User",
            DoctorName = a.DoctorName ?? string.Empty,
            Specialty = a.Specialty ?? string.Empty,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Purpose = a.Purpose ?? string.Empty,
            Status = a.Status
        }).ToList();

        return Ok(appointmentDtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving assistant pending appointments");
        return StatusCode(500, "Error retrieving appointments");
    }
}


[HttpGet("assistant/{assistantId}/pending")]
[Authorize(Roles = "Assistant")]
public async Task<ActionResult<List<AppointmentResponseDto>>> GetAssistantPendingAppointments(Guid assistantId)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser.Id != assistantId)
        {
            return Forbid("You can only view your own appointments");
        }

        var appointments = await _appointmentRepository.GetPendingAppointmentsForAssistantAsync(assistantId);
        
        var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown User",
            DoctorName = a.DoctorName ?? string.Empty,
            Specialty = a.Specialty ?? string.Empty,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Purpose = a.Purpose ?? string.Empty,
            Status = a.Status
        }).ToList();

        return Ok(appointmentDtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving assistant pending appointments");
        return StatusCode(500, "Error retrieving appointments");
    }
}

[HttpGet("assistant/{assistantId}/approved")]
[Authorize(Roles = "Assistant")]
public async Task<ActionResult<List<AppointmentResponseDto>>> GetAssistantApprovedAppointments(Guid assistantId)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
       
        if (currentUser.Id != assistantId)
        {
            return Forbid("You can only view your own appointments");
        }

        var appointments = await _appointmentRepository.GetApprovedAppointmentsForAssistantAsync(assistantId);
        
        var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown User",
            DoctorName = a.DoctorName ?? string.Empty,
            Specialty = a.Specialty ?? string.Empty,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Purpose = a.Purpose ?? string.Empty,
            Status = a.Status
        }).ToList();

        return Ok(appointmentDtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving assistant approved appointments");
        return StatusCode(500, "Error retrieving appointments");
    }
}

        [HttpPut("{id}/assistant-approve")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult> AssistantApprove(Guid id)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync(); // Tani do të funksionojë
                var result = await _appointmentRepository.AssistantApproveAsync(id, currentUser.Id);
                
                if (!result) 
                    return BadRequest("Not authorized to approve this appointment or appointment not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving appointment {AppointmentId}", id);
                return StatusCode(500, "Error approving appointment");
            }
        }

 [HttpPut("{id}/assistant-reject")]
[Authorize(Roles = "Assistant")]
public async Task<ActionResult> AssistantReject(Guid id, [FromBody] RejectAppointmentDto rejectDto)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        var result = await _appointmentRepository.AssistantRejectAsync(id, currentUser.Id, rejectDto.RejectionReason);
        
        if (!result) 
            return BadRequest("Not authorized to reject this appointment or appointment not found");

        return NoContent();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error rejecting appointment {AppointmentId}", id);
        return StatusCode(500, "Error rejecting appointment");
    }
}

public class RejectAppointmentDto
{
    public string RejectionReason { get; set; } = string.Empty;
}
    }
}