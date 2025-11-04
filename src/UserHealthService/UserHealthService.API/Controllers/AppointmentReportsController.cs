using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.DTOs.Doctors;
using UserHealthService.Application.Interfaces;
using UserHealthService.Application.Services;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentReportsController : ControllerBase
    {
        private readonly IAppointmentReportService _reportService;
        private readonly IAuthService _authService;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentReportsController> _logger;
        private readonly IMapper _mapper;

        public AppointmentReportsController(
            IAppointmentReportService reportService,
            IAuthService authService,
            IDoctorRepository doctorRepository,
            IAppointmentService appointmentService,
            ILogger<AppointmentReportsController> logger,
            IMapper mapper)
        {
            _reportService = reportService;
            _authService = authService;
            _appointmentService = appointmentService;
            _logger = logger;
            _mapper = mapper;
            _doctorRepository = doctorRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppointmentReportResponseDto>>> GetAllReports()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                IEnumerable<AppointmentReport> reports;

                if (currentUser.Type == UserHealthService.Domain.Enums.UserType.HealthcareProvider)
                {   
                    reports = await _reportService.GetByDoctorIdAsync(currentUser.Id);
                }
                else if (currentUser.Type == UserHealthService.Domain.Enums.UserType.Assistant)
                {
                    reports = await _reportService.GetAllAsync();
                }
                else
                {
                    reports = await _reportService.GetByUserIdAsync(currentUser.Id);
                }

                // MAP ENTITIES TO DTOs BEFORE RETURNING
                var responseDtos = _mapper.Map<IEnumerable<AppointmentReportResponseDto>>(reports);
                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reports");
                return StatusCode(500, "Error retrieving reports");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AppointmentReportResponseDto>> CreateReport(
            [FromBody] AppointmentReportCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Validation failed", errors = ModelState });

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Type != UserType.HealthcareProvider && currentUser.Type != UserType.Assistant)
                    return Forbid("Only doctors and assistants can create reports");

                if (!Guid.TryParse(dto.AppointmentId, out var appointmentId))
                    return BadRequest(new { message = "Invalid AppointmentId" });

                if (!Guid.TryParse(dto.UserId, out var userId))
                    return BadRequest(new { message = "Invalid UserId" });

                if (!Guid.TryParse(dto.DoctorId, out var doctorId))
                    return BadRequest(new { message = "Invalid DoctorId" });

                var appointment = await _appointmentService.GetByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                if (appointment.Status != AppointmentStatus.Approved)
                    return BadRequest(new { message = "Can only create report for approved appointments" });

                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    return BadRequest(new { message = "Doctor not found in Doctors table", doctorId });

                if (doctorId != currentUser.Id && currentUser.Type != UserType.Assistant)
                    return Forbid("You can only create reports for yourself");

                var report = _mapper.Map<AppointmentReport>(dto);
                report.Id = dto.Id ?? Guid.NewGuid();
                report.AppointmentId = appointmentId;
                report.UserId = userId;
                report.DoctorId = doctorId;
                report.ReportDate = string.IsNullOrWhiteSpace(dto.ReportDate)
                    ? DateTime.UtcNow
                    : DateTime.Parse(dto.ReportDate);
                report.CreatedAt = DateTime.UtcNow;
                report.UpdatedAt = DateTime.UtcNow;

                var createdReport = await _reportService.CreateAsync(report);
                var responseDto = _mapper.Map<AppointmentReportResponseDto>(createdReport);

                return CreatedAtAction(nameof(GetReportById), new { id = responseDto.Id }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report");
                return StatusCode(500, new { message = "Failed to save report", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AppointmentReportResponseDto>> GetReportById(Guid id)
        {
            try
            {
                var report = await _reportService.GetByIdAsync(id);
                if (report == null)
                    return NotFound();

                // MAP ENTITY TO DTO
                var responseDto = _mapper.Map<AppointmentReportResponseDto>(report);
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {ReportId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("appointment/{appointmentId}")]
        [Authorize]
        public async Task<ActionResult<AppointmentReportResponseDto>> GetReportByAppointmentId(Guid appointmentId)
        {
            try
            {
                var report = await _reportService.GetByAppointmentIdAsync(appointmentId);
                if (report == null)
                    return NotFound();

                // MAP ENTITY TO DTO
                var responseDto = _mapper.Map<AppointmentReportResponseDto>(report);
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report for appointment {AppointmentId}", appointmentId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppointmentReportResponseDto>>> GetReportsByUserId(Guid userId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();

                if (currentUser.Id != userId && currentUser.Type != UserHealthService.Domain.Enums.UserType.HealthcareProvider)
                {
                    return Forbid("You can only view your own reports");
                }

                var reports = await _reportService.GetByUserIdAsync(userId);

                // MAP ENTITIES TO DTOs
                var responseDtos = _mapper.Map<IEnumerable<AppointmentReportResponseDto>>(reports);
                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports for user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Doctor,Assistant")]
        public async Task<ActionResult<IEnumerable<AppointmentReportResponseDto>>> GetReportsByDoctorId(Guid doctorId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();

                if (currentUser.Id != doctorId && currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
                {
                    return Forbid("You can only view your own reports");
                }

                var reports = await _reportService.GetByDoctorIdAsync(doctorId);

                // MAP ENTITIES TO DTOs
                var responseDtos = _mapper.Map<IEnumerable<AppointmentReportResponseDto>>(reports);
                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports for doctor {DoctorId}", doctorId);
                return StatusCode(500, "Error retrieving reports");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<AppointmentReportResponseDto>> UpdateReport(
            Guid id,
            [FromBody] AppointmentReportUpdateDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest("ID mismatch");

                var currentUser = await _authService.GetCurrentUserAsync();
                var existingReport = await _reportService.GetByIdAsync(id);

                if (existingReport == null)
                    return NotFound();

                if (existingReport.DoctorId != currentUser.Id &&
                    currentUser.Type != UserType.Assistant)
                {
                    return Forbid("You can only update your own reports");
                }

                // Map update to existing entity
                _mapper.Map(dto, existingReport);
                existingReport.UpdatedAt = DateTime.UtcNow;

                var updatedReport = await _reportService.UpdateAsync(existingReport);
                var responseDto = _mapper.Map<AppointmentReportResponseDto>(updatedReport);

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReport(Guid id)
        {
            try
            {
                _logger.LogInformation("=== DELETE REPORT STARTED ===");

                var currentUser = await _authService.GetCurrentUserAsync();

                _logger.LogInformation("Current User - ID: {UserId}, Type: {UserType}, TypeInt: {UserTypeInt}",
                    currentUser.Id, currentUser.Type, (int)currentUser.Type);

                var existingReport = await _reportService.GetByIdAsync(id);
                if (existingReport == null)
                {
                    _logger.LogWarning("Report {ReportId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Report Found - ID: {ReportId}, DoctorId: {ReportDoctorId}",
                    existingReport.Id, existingReport.DoctorId);

                // Log detailed comparison
                _logger.LogInformation("=== PERMISSION CHECK ===");
                _logger.LogInformation("DoctorId Match: {DoctorIdMatch} (Report: {ReportDoctorId} vs User: {UserId})",
                    existingReport.DoctorId == currentUser.Id, existingReport.DoctorId, currentUser.Id);
                _logger.LogInformation("Is Assistant: {IsAssistant} (UserType: {UserType}, UserTypeInt: {UserTypeInt})",
                    currentUser.Type == UserType.Assistant, currentUser.Type, (int)currentUser.Type);

                _logger.LogInformation("Assistant Enum Values - UserType.Assistant: {AssistantValue}, CurrentUser.Type: {CurrentValue}",
                    (int)UserType.Assistant, (int)currentUser.Type);

                bool canDelete = existingReport.DoctorId == currentUser.Id
                  || currentUser.Type == UserType.Assistant;
                _logger.LogInformation("Can Delete: {CanDelete}", canDelete);

                if (!canDelete)
                {
                    _logger.LogWarning("=== DELETE FORBIDDEN ===");
                    _logger.LogWarning("User {UserId} cannot delete report {ReportId}", currentUser.Id, id);
                    _logger.LogWarning("Reason: Not report owner and not assistant");
                    return Forbid("You can only delete your own reports");
                }

                _logger.LogInformation("=== PROCEEDING WITH DELETE ===");
                var result = await _reportService.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Delete operation failed for report {ReportId}", id);
                    return NotFound();
                }

                _logger.LogInformation("=== DELETE SUCCESSFUL ===");
                _logger.LogInformation("Report {ReportId} deleted by user {UserId}", id, currentUser.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== DELETE ERROR ===");
                _logger.LogError("Error deleting report {ReportId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{id}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var report = await _reportService.GetByIdAsync(id);
                if (report == null)
                    return NotFound();

                // FIX: Convert Guid → string
                var canAccess = report.UserId == currentUser.Id
                             || report.DoctorId == currentUser.Id
                             || currentUser.Type == UserType.Assistant;

                if (!canAccess)
                    return Forbid("You don't have permission to download this report");

                var pdfBytes = await _reportService.GeneratePdfAsync(id);
                return File(pdfBytes, "application/pdf", $"report-{id}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF generation failed for report {Id}", id);
                return BadRequest("Failed to generate PDF");
            }
        }

        [HttpPost("assistant/{assistantId}")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<AppointmentReportResponseDto>> CreateAssistantReport(Guid assistantId, [FromBody] AppointmentReportCreateDto dto)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();

                if (currentUser.Id != assistantId)
                {
                    return Forbid("You can only create reports for your own appointments");
                }

                // Use the same logic as the main CreateReport method but with assistant validation
                if (!Guid.TryParse(dto.AppointmentId, out var appointmentId))
                    return BadRequest(new { message = "Invalid AppointmentId" });

                if (!Guid.TryParse(dto.UserId, out var userId))
                    return BadRequest(new { message = "Invalid UserId" });

                if (!Guid.TryParse(dto.DoctorId, out var doctorId))
                    return BadRequest(new { message = "Invalid DoctorId" });

                var appointment = await _appointmentService.GetByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound(new { message = "Appointment not found" });

                if (appointment.Status != AppointmentStatus.Approved)
                    return BadRequest(new { message = "Can only create report for approved appointments" });

                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    return BadRequest(new { message = "Doctor not found in Doctors table", doctorId });

                var report = _mapper.Map<AppointmentReport>(dto);
                report.Id = dto.Id ?? Guid.NewGuid();
                report.AppointmentId = appointmentId;
                report.UserId = userId;
                report.DoctorId = doctorId;
                report.ReportDate = string.IsNullOrWhiteSpace(dto.ReportDate)
                    ? DateTime.UtcNow
                    : DateTime.Parse(dto.ReportDate);
                report.CreatedAt = DateTime.UtcNow;
                report.UpdatedAt = DateTime.UtcNow;

                var createdReport = await _reportService.CreateAsync(report);
                var responseDto = _mapper.Map<AppointmentReportResponseDto>(createdReport);

                return CreatedAtAction(nameof(GetReportById), new { id = responseDto.Id }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assistant report");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("assistant/{assistantId}")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<IEnumerable<AppointmentReportResponseDto>>> GetAssistantReports(Guid assistantId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser.Id != assistantId)
                {
                    return Forbid("You can only view your own reports");
                }
                var reports = await _reportService.GetByDoctorIdAsync(assistantId);
                var responseDtos = _mapper.Map<IEnumerable<AppointmentReportResponseDto>>(reports);
                return Ok(responseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assistant reports for {AssistantId}", assistantId);
                return StatusCode(500, "Error retrieving reports");
            }
        }
    }
}