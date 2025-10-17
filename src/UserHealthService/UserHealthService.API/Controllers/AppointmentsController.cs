using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.Interfaces;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
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

        [HttpPost]
        public async Task<ActionResult<AppointmentResponseDto>> Create(AppointmentCreateDto createDto)
        {
            var createdAppointment = await _appointmentService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdAppointment.Id }, createdAppointment);
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
    }
}