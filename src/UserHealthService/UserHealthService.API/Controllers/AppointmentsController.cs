using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using AutoMapper;
using UserHealthService.Infrastructure.Data;

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
        private readonly IPDFReportService _pdfReportService;
        private readonly IMapper _mapper;
        private readonly UserHealthDbContext _context;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentRepository appointmentRepository,
            IUserService userService,
            IAuthService authService,
            IPDFReportService pdfReportService,
            ILogger<AppointmentsController> logger,
            IMapper mapper,
            UserHealthDbContext context)
        {
            _appointmentService = appointmentService;
            _appointmentRepository = appointmentRepository;
            _userService = userService;
            _authService = authService;
            _pdfReportService = pdfReportService;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAll()
        {
            var appointments = await _appointmentService.GetAllAsync();
            return Ok(appointments);
        }

        // GET: api/appointments/doctor/{doctorId}
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByDoctor(Guid doctorId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Id != doctorId && currentUser.Type != UserType.Assistant)
                    return Forbid("You can only view your own appointments");

                var appointments = await _appointmentService.GetByDoctorIdAsync(doctorId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for doctor {DoctorId}", doctorId);
                return StatusCode(500, "Error retrieving appointments");
            }
        }

        // GET: api/appointments/doctor/{doctorId}/approved
        [HttpGet("doctor/{doctorId}/approved")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetApprovedAppointmentsByDoctor(Guid doctorId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Id != doctorId && currentUser.Type != UserType.Assistant)
                    return Forbid("You can only view your own appointments");

                var doctor = await _context.Doctors.FindAsync(doctorId);
                if (doctor == null) return NotFound("Doctor not found");

                var allAppointments = await _appointmentRepository.GetAllAsync();
                var approvedAppointments = allAppointments
                    .Where(a => a.DoctorName == doctor.Name && a.Status == AppointmentStatus.Approved)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<AppointmentResponseDto>>(approvedAppointments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approved appointments for doctor {DoctorId}", doctorId);
                return StatusCode(500, "Error retrieving appointments");
            }
        }



        // GET: api/appointments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentResponseDto>> GetById(Guid id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        // GET: api/appointments/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetByUserId(Guid userId)
        {
            var appointments = await _appointmentService.GetByUserIdAsync(userId);
            return Ok(appointments);
        }

        // GET: api/appointments/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetUpcoming()
        {
            var appointments = await _appointmentService.GetUpcomingAsync();
            return Ok(appointments);
        }

        // GET: api/appointments/filter?fromDate=...&toDate=...
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetByDateRange(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var appointments = await _appointmentService.GetAppointmentsByDateRangeAsync(fromDate, toDate);
            return Ok(appointments);
        }

        // POST: api/appointments
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

        // PUT: api/appointments/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<AppointmentResponseDto>> Update(Guid id, AppointmentUpdateDto updateDto)
        {
            var updatedAppointment = await _appointmentService.UpdateAsync(id, updateDto);
            if (updatedAppointment == null) return NotFound();
            return Ok(updatedAppointment);
        }

        // PUT: api/appointments/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> Cancel(Guid id)
        {
            var result = await _appointmentService.CancelAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/appointments/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var result = await _appointmentService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // GET: api/appointments/pending
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

        // GET: api/appointments/approved
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

        // GET: api/appointments/rejected
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

        // GET: api/appointments/user/{userId}/upcoming/count
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

        // PUT: api/appointments/{id}/approve
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

        // GET: api/appointments/assistant/pending
        [HttpGet("assistant/pending")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetAssistantPendingAppointments()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var appointments = await _appointmentRepository.GetPendingAppointmentsForAssistantAsync(currentUser.Id);

                var dtos = appointments.Select(a => new AppointmentResponseDto
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

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assistant pending appointments");
                return StatusCode(500, "Error retrieving appointments");
            }
        }

        // GET: api/appointments/assistant/{assistantId}/pending
        [HttpGet("assistant/{assistantId}/pending")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetAssistantPendingAppointments(Guid assistantId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Id != assistantId)
                    return Forbid("You can only view your own appointments");

                var appointments = await _appointmentRepository.GetPendingAppointmentsForAssistantAsync(assistantId);
                var dtos = appointments.Select(a => new AppointmentResponseDto
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

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assistant pending appointments");
                return StatusCode(500, "Error retrieving appointments");
            }
        }

        // GET: api/appointments/assistant/{assistantId}/approved
        [HttpGet("assistant/{assistantId}/approved")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<List<AppointmentResponseDto>>> GetAssistantApprovedAppointments(Guid assistantId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Id != assistantId)
                    return Forbid("You can only view your own appointments");

                var appointments = await _appointmentRepository.GetApprovedAppointmentsForAssistantAsync(assistantId);
                var dtos = appointments.Select(a => new AppointmentResponseDto
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

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assistant approved appointments");
                return StatusCode(500, "Error retrieving appointments");
            }
        }

        // PUT: api/appointments/{id}/assistant-approve
        [HttpPut("{id}/assistant-approve")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult> AssistantApprove(Guid id)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var result = await _appointmentRepository.AssistantApproveAsync(id, currentUser.Id);
                if (!result) return BadRequest("Not authorized to approve this appointment or appointment not found");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving appointment {AppointmentId}", id);
                return StatusCode(500, "Error approving appointment");
            }
        }

        // PUT: api/appointments/{id}/assistant-reject
        [HttpPut("{id}/assistant-reject")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult> AssistantReject(Guid id, [FromBody] RejectAppointmentDto rejectDto)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var result = await _appointmentRepository.AssistantRejectAsync(id, currentUser.Id, rejectDto.RejectionReason);
                if (!result) return BadRequest("Not authorized to reject this appointment or appointment not found");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting appointment {AppointmentId}", id);
                return StatusCode(500, "Error rejecting appointment");
            }
        }
    }

    // DTO for rejection
    public class RejectAppointmentDto
    {
        public string RejectionReason { get; set; } = string.Empty;
    }
}