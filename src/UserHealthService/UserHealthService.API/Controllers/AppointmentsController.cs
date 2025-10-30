using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using AutoMapper;
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
           private readonly IAppointmentReportService _reportService;
        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentRepository appointmentRepository,
            IUserService userService,
            IAuthService authService,
  IPDFReportService pdfReportService,
IAppointmentReportService reportService,

            ILogger<AppointmentsController> logger ,IMapper mapper) 
        {
            _appointmentService = appointmentService;
            _appointmentRepository = appointmentRepository;
            _userService = userService;
            _authService = authService;
            _pdfReportService = pdfReportService;
            _reportService = reportService;
                _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAll()
        {
            var appointments = await _appointmentService.GetAllAsync();
            return Ok(appointments);
        }
        [HttpGet("doctor/{doctorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointmentsByDoctor(Guid doctorId)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();

                if (currentUser.Id != doctorId && currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
                {
                    return Forbid("You can only view your own appointments");
                }

                var appointments = await _appointmentService.GetByDoctorIdAsync(doctorId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for doctor {DoctorId}", doctorId);
                return StatusCode(500, "Error retrieving appointments");
            }
        }

[HttpGet("doctor/{doctorId}/approved")]
[Authorize]
public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetApprovedAppointmentsByDoctor(Guid doctorId)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser.Id != doctorId && currentUser.Type != UserHealthService.Domain.Enums.UserType.Assistant)
        {
            return Forbid("You can only view your own appointments");
        }

        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
        var approvedAppointments = appointments.Where(a => a.Status == AppointmentStatus.Approved);
        
        return Ok(_mapper.Map<IEnumerable<AppointmentResponseDto>>(approvedAppointments));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving approved appointments for doctor {DoctorId}", doctorId);
        return StatusCode(500, "Error retrieving appointments");
    }
}
 [HttpGet("test-pdf")]
        [AllowAnonymous] 
        public async Task<IActionResult> TestPDF()
        {
            try
            {
              
                var sampleReport = new AppointmentReportResponseDto
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    UserName = "Valza Mustafa",
                    DoctorId = Guid.NewGuid(),
                    DoctorName = "Dr. Valza",
                    Specialty = "Cardiology",
                    ReportDate = DateTime.UtcNow,
                    Diagnosis = "Hypertension Stage 1",
                    Symptoms = "Elevated blood pressure, occasional headaches, dizziness",
                    Treatment = "Lifestyle modifications and medication management",
                    Medications = "Lisinopril 10mg daily, Aspirin 81mg daily",
                    Notes = "Patient advised to reduce sodium intake, exercise regularly (30 mins daily), and monitor blood pressure at home.",
                    Recommendations = "Follow up in 3 months for blood pressure check. Return immediately if experiencing chest pain or severe headaches.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow
                };

                var pdfBytes = await _pdfReportService.GenerateAppointmentReportPDFAsync(sampleReport);
                var fileName = _pdfReportService.GetReportFileName(sampleReport);
               
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test PDF");
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
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
                var currentUser = await _authService.GetCurrentUserAsync(); 
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

[HttpGet("assistant/{assistantId}/reports")]
[Authorize(Roles = "Assistant")]
public async Task<ActionResult<List<AppointmentReportResponseDto>>> GetAssistantAppointmentReports(Guid assistantId)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser.Id != assistantId)
        {
            return Forbid("You can only view your own reports");
        }

        return Ok(new List<AppointmentReportResponseDto>());
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving assistant appointment reports");
        return StatusCode(500, "Error retrieving reports");
    }
}

[HttpPost("assistant/{assistantId}/reports")]
[Authorize(Roles = "Assistant")]
public async Task<ActionResult<AppointmentReport>> CreateAssistantReport(Guid assistantId, [FromBody] AppointmentReport report)
{
    try
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser.Id != assistantId)
        {
            return Forbid("You can only create reports for your appointments");
        }

        var createdReport = new AppointmentReport
        {
            Id = Guid.NewGuid(),
            AppointmentId = report.AppointmentId,
            UserId = report.UserId,
            DoctorId = currentUser.Id,
            ReportDate = DateTime.UtcNow,
            Diagnosis = report.Diagnosis,
            Symptoms = report.Symptoms,
            Treatment = report.Treatment,
            Medications = report.Medications,
            Notes = report.Notes,
            Recommendations = report.Recommendations,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetReportById), new { id = createdReport.Id }, createdReport);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating assistant report");
        return StatusCode(500, "Error creating report");
    }
}
        [HttpGet("reports/{id}")]
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
                _logger.LogError(ex, "Error retrieving report");
                return StatusCode(500, "Error retrieving report");
            }
        }
     

[HttpGet("reports-test")]
[AllowAnonymous]
public ActionResult<string> ReportsTest()
{
    return Ok("Reports functionality is accessible via AppointmentsController!");
}

    [HttpGet("reports")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AppointmentReport>>> GetReports()
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
            _logger.LogError(ex, "Error retrieving reports via appointments controller");
            return StatusCode(500, "Error retrieving reports");
        }
    }
      

[HttpPut("reports/{id}")]
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
        return StatusCode(500, "Error updating report");
    }
}

[HttpDelete("reports/{id}")]
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
        return StatusCode(500, "Error deleting report");
    }
}

[HttpGet("reports/{id}/download")]
[Authorize]
public async Task<IActionResult> DownloadReportPdf(Guid id)
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
        return StatusCode(500, "Error generating PDF");
    }
}
    }public class RejectAppointmentDto
{
    public string RejectionReason { get; set; } = string.Empty;
}
    }
