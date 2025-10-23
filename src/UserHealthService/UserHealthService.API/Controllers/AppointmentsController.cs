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

        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentRepository appointmentRepository,
            IUserService userService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _appointmentRepository = appointmentRepository;
            _userService = userService;
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

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Reject(Guid id)
        {
            try
            {
                var result = await _appointmentRepository.RejectAsync(id);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting appointment {AppointmentId}", id);
                return StatusCode(500, "Error rejecting appointment");
            }
        }
    }
}