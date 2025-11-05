using Grpc.Core;
using MedicationService.Application.DTOs.Interactions;
using MedicationService.Application.DTOs.Prescriptions;
using MedicationService.Application.DTOs.Reminders;
using MedicationService.Application.DTOs.Schedules;
using MedicationService.Application.Interfaces;
using MedicationService.Application.Services;
using MedicationService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static MedicationService.Protos.Medication;

namespace MedicationService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        public IMedicationReminderService _medicationReminderService;
        public IMedicationScheduleService _medicationScheduleService;
        private readonly ILogger<SchedulesController> _logger;
        public SchedulesController(IMedicationScheduleService medicationScheduleService, ILogger<SchedulesController> logger, IMedicationReminderService medicationReminderService)
        {
            _medicationReminderService = medicationReminderService;
            _medicationScheduleService = medicationScheduleService;
            _logger = logger;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetAll()
        {
            try
            {
                var schedules = await _medicationScheduleService.GetAllAsync();
                return Ok(schedules);
            } catch (Exception ex)
            {
                _logger.LogError($"{ex} The schedules couldnt be retrieved");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ScheduleResponseDto>> GetById(Guid id)
        {
            try
            {
                var scheduleById = await _medicationScheduleService.GetByIdAsync(id);
                if (scheduleById == null)
                {
                    return NotFound(scheduleById);
                }
                return Ok(scheduleById);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Schedule with Id : {id} couldnt be found . ");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("medication/{medicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetByMedication(Guid medicationId)
        {
            try
            {
                var scheduleByMedicationId = await _medicationScheduleService.GetByMedicationIdAsync(medicationId);
                if (scheduleByMedicationId == null)
                {
                    return NotFound(scheduleByMedicationId);
                }
                return Ok(scheduleByMedicationId);
            } catch (Exception ex)
            {
                _logger.LogError($"{ex} Schedules with Medication Id : {medicationId} couldnt be found . ");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetUpcompingSchedules()
        {
            try
            {
                var now = DateTime.UtcNow;
                var next24h = now.AddHours(24);

                var schedules = await _medicationScheduleService.GetUpcomingSchedulesAsync(now, next24h);

                if (!schedules.Any())
                    return NotFound("No upcoming schedules found.");

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming schedules");
                return StatusCode(500, "Internal server error");
            }

        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ScheduleResponseDto>> Create([FromBody] ScheduleCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var schedule = await _medicationScheduleService.CreateAsync(createDto);

                for (int dayOffset = 0; dayOffset < 7; dayOffset++)
                {
                    var scheduledTime = DateTime.UtcNow.Date
               .AddDays(dayOffset)
               .Add(schedule.TimeOfDay);
                    var reminder = new ReminderCreateDto
                    {
                        MedicationId = schedule.MedicationId,
                        ScheduleId = schedule.Id,
                        ScheduledTime = scheduledTime,
                        Message = $"Time to take {schedule.MedicationName ?? "your medication"}",
                        NotificationChannel = "in-app"
                    };
    
                    await _medicationReminderService.CreateAsync(reminder);
                }

                return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schedule couldn't be created");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ScheduleResponseDto>> Update(Guid id, [FromBody] ScheduleUpdateDto updateDto)
        {

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var schedule = await _medicationScheduleService.UpdateAsync(id, updateDto);
                if (schedule== null)
                    return NotFound($"Schedule with ID {id} not found");

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _medicationScheduleService.DeleteAsync(id);
                if (!result)
                    return NotFound($"Schedule with ID {id} not found");

                return NoContent();
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex}Schedule with Id:{id} couldnt be deleted");
               return StatusCode(500, "Internal Server Error");
            }
        }


    }


}
