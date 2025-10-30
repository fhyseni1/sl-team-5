using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentReportsController : ControllerBase
    {
        private readonly IAppointmentReportService _reportService;

        public AppointmentReportsController(IAppointmentReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<AppointmentReport>> CreateReport(AppointmentReport report)
        {
            try
            {
                var createdReport = await _reportService.CreateAsync(report);
                return CreatedAtAction(nameof(GetReportById), new { id = createdReport.Id }, createdReport);
            }
            catch (Exception ex)
            {
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppointmentReport>>> GetReportsByUserId(Guid userId)
        {
            try
            {
                var reports = await _reportService.GetByUserIdAsync(userId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppointmentReport>>> GetReportsByDoctorId(Guid doctorId)
        {
            try
            {
                var reports = await _reportService.GetByDoctorIdAsync(doctorId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<AppointmentReport>> UpdateReport(Guid id, AppointmentReport report)
        {
            try
            {
                if (id != report.Id)
                    return BadRequest("ID mismatch");

                var updatedReport = await _reportService.UpdateAsync(report);
                return Ok(updatedReport);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> DeleteReport(Guid id)
        {
            try
            {
                var result = await _reportService.DeleteAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            try
            {
                var pdfBytes = await _reportService.GeneratePdfAsync(id);
                return File(pdfBytes, "application/pdf", $"appointment-report-{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}