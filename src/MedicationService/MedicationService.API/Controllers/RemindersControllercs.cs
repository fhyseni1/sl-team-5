using MedicationService.Application.DTOs.Reminders;
using MedicationService.Application.Interfaces;
using MedicationService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MedicationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly IMedicationReminderService _service;
        private readonly ILogger<RemindersController> _logger;

        public RemindersController(IMedicationReminderService service, ILogger<RemindersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetAll()
        {
            try
            {
                var reminders = await _service.GetAllAsync();
                return Ok(reminders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error getting all reminders.");
                return StatusCode(500, "Internal server error while fetching reminders.");
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReminderResponseDto>> GetById(Guid id)
        {
            try
            {
                var reminder = await _service.GetByIdAsync(id);
                if (reminder == null)
                    return NotFound($"Reminder with ID {id} not found.");
                return Ok(reminder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error getting reminder by ID {id}.");
                return StatusCode(500, "Internal server error while fetching reminder by ID.");
            }
        }

        [HttpGet("medication/{medicationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetByMedication(Guid medicationId)
        {
            try
            {
                var reminders = await _service.GetByMedicationIdAsync(medicationId);
                return Ok(reminders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error getting reminders by medication ID {medicationId}.");
                return StatusCode(500, "Internal server error while fetching reminders by medication.");
            }
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetByStatus(ReminderStatus status)
        {
            try
            {
                var reminders = await _service.GetByStatusAsync(status);
                return Ok(reminders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error getting reminders by status {status}.");
                return StatusCode(500, "Internal server error while fetching reminders by status.");
            }
        }

        [HttpGet("missed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetMissedReminders()
        {
            try
            {
                var missed = await _service.GetMissedRemindersAsync();
                return Ok(missed);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error getting missed reminders.");
                return StatusCode(500, "Internal server error while fetching missed reminders.");
            }
        }

        [HttpGet("pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetPendingReminders()
        {
            try
            {
                var pending = await _service.GetPendingRemindersAsync();
                return Ok(pending);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error getting pending reminders.");
                return StatusCode(500, "Internal server error while fetching pending reminders.");
            }
        }

        [HttpGet("upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetUpcomingReminders()
        {
            try
            {
                var beforeTime = DateTime.UtcNow.AddHours(24);
                var upcoming = await _service.GetUpcomingRemindersAsync(beforeTime);
                if (upcoming == null || !upcoming.Any())
                    return NoContent();
                return Ok(upcoming);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error getting upcoming reminders.");
                return StatusCode(500, "Internal server error while fetching upcoming reminders.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReminderResponseDto>> Create([FromBody] ReminderCreateDto dto)
        {
            try
            {
                var reminder = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = reminder.Id }, reminder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error creating reminder.");
                return StatusCode(500, "Internal server error while creating reminder.");
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReminderResponseDto>> Update(Guid id, [FromBody] ReminderUpdateDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null)
                    return NotFound($"Reminder with ID {id} not found.");
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error updating reminder with ID {id}.");
                return StatusCode(500, "Internal server error while updating reminder.");
            }
        }

        [HttpPut("{id:guid}/snooze")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ReminderResponseDto>> Snooze(Guid id, [FromBody] ReminderUpdateDto dto)
        {
            try
            {
                var snoozed = await _service.SnoozeReminder(id, dto);
                if (snoozed == null)
                    return NotFound($"Reminder with ID {id} not found.");
                return Ok(snoozed);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error snoozing reminder with ID {id}.");
                return StatusCode(500, "Internal server error while snoozing reminder.");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success)
                    return NotFound($"Reminder with ID {id} not found.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex} Error deleting reminder with ID {id}.");
                return StatusCode(500, "Internal server error while deleting reminder.");
            }
        }
    }
}
