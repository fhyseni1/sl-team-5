using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentReportsController : ControllerBase
    {
        private readonly IAppointmentReportService _reportService;
        private readonly IAuthService _authService;
        private readonly ILogger<AppointmentReportsController> _logger;

        public AppointmentReportsController(
            IAppointmentReportService reportService,
            IAuthService authService,
            ILogger<AppointmentReportsController> logger)
        {
            _reportService = reportService;
            _authService = authService;
            _logger = logger;
        }
       [HttpGet]
[Authorize]
public async Task<ActionResult<IEnumerable<AppointmentReport>>> GetAllReports()
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

        return Ok(reports);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving all reports");
        return StatusCode(500, "Error retrieving reports");
    }
}
        [HttpPost]
        [Authorize(Roles = "Doctor,Assistant")]
        public async Task<ActionResult<AppointmentReport>> CreateReport([FromBody] AppointmentReport report)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
              
                if (currentUser.Type != UserHealthService.Domain.Enums.UserType.HealthcareProvider && 
                    currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
                {
                    return Forbid("Only doctors and assistants can create reports");
                }

                var createdReport = await _reportService.CreateAsync(report);
                return CreatedAtAction(nameof(GetReportById), new { id = createdReport.Id }, createdReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment report");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AppointmentReport>> GetReportById(Guid id)
        {
            try
            {
                var report = await _reportService.GetByIdAsync(id);
                if (report == null)
                    return NotFound();

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {ReportId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("appointment/{appointmentId}")]
        [Authorize]
        public async Task<ActionResult<AppointmentReport>> GetReportByAppointmentId(Guid appointmentId)
        {
            try
            {
                var report = await _reportService.GetByAppointmentIdAsync(appointmentId);
                if (report == null)
                    return NotFound();

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report for appointment {AppointmentId}", appointmentId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppointmentReport>>> GetReportsByUserId(Guid userId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
             
                if (currentUser.Id != userId && currentUser.Type != UserHealthService.Domain.Enums.UserType.HealthcareProvider)
                {
                    return Forbid("You can only view your own reports");
                }

                var reports = await _reportService.GetByUserIdAsync(userId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports for user {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
        }

      [HttpGet("doctor/{doctorId}")]
[Authorize(Roles = "Doctor,Assistant")]
public async Task<ActionResult<IEnumerable<AppointmentReport>>> GetReportsByDoctorId(Guid doctorId)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser.Id != doctorId && currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
        {
            return Forbid("You can only view your own reports");
        }

        var reports = await _reportService.GetByDoctorIdAsync(doctorId);
        return Ok(reports);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving reports for doctor {DoctorId}", doctorId);
        return StatusCode(500, "Error retrieving reports");
    }
}

        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor,Assistant")]
        public async Task<ActionResult<AppointmentReport>> UpdateReport(Guid id, [FromBody] AppointmentReport report)
        {
            try
            {
                if (id != report.Id)
                    return BadRequest("ID mismatch");

                var currentUser = await _authService.GetCurrentUserAsync();
               
                var existingReport = await _reportService.GetByIdAsync(id);
                if (existingReport == null)
                    return NotFound();

                if (existingReport.DoctorId != currentUser.Id && currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
                {
                    return Forbid("You can only update your own reports");
                }

                var updatedReport = await _reportService.UpdateAsync(report);
                return Ok(updatedReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Assistant")]
        public async Task<ActionResult> DeleteReport(Guid id)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
               
                var existingReport = await _reportService.GetByIdAsync(id);
                if (existingReport == null)
                    return NotFound();

                if (existingReport.DoctorId != currentUser.Id && currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
                {
                    return Forbid("You can only delete your own reports");
                }

                var result = await _reportService.DeleteAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
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

                if (report.UserId != currentUser.Id && report.DoctorId != currentUser.Id && 
                    currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
                {
                    return Forbid("You don't have permission to download this report");
                }

                var pdfBytes = await _reportService.GeneratePdfAsync(id);
                return File(pdfBytes, "application/pdf", $"appointment-report-{id}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for report {ReportId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("assistant/{assistantId}")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<AppointmentReport>> CreateAssistantReport(Guid assistantId, [FromBody] AppointmentReport report)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                
                if (currentUser.Id != assistantId)
                {
                    return Forbid("You can only create reports for your own appointments");
                }

                var createdReport = await _reportService.CreateAsync(report);
                return CreatedAtAction(nameof(GetReportById), new { id = createdReport.Id }, createdReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assistant report");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("assistant/{assistantId}")]
        [Authorize(Roles = "Assistant")]
        public async Task<ActionResult<IEnumerable<AppointmentReport>>> GetAssistantReports(Guid assistantId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                
                if (currentUser.Id != assistantId)
                {
                    return Forbid("You can only view your own reports");
                }

                var reports = await _reportService.GetByDoctorIdAsync(assistantId); 
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assistant reports for {AssistantId}", assistantId);
                return StatusCode(500, "Error retrieving reports");
            }
        }
    }
}