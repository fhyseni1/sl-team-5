using MedicationService.Application.DTOs.Medications;
using MedicationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MedicationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationService _medicationService;
        private readonly ILogger<MedicationsController> _logger;

        public MedicationsController(
            IMedicationService medicationService,
            ILogger<MedicationsController> logger)
        {
            _medicationService = medicationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicationResponseDto>>> GetAll()
        {
            try
            {
                var medications = await _medicationService.GetAllAsync();
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all medications");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicationResponseDto>> GetById(Guid id)
        {
            try
            {
                var medication = await _medicationService.GetByIdAsync(id);
                if (medication == null)
                    return NotFound($"Medication with ID {id} not found");

                return Ok(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<MedicationResponseDto>>> GetByUserId(Guid userId)
        {
            try
            {
                var medications = await _medicationService.GetByUserIdAsync(userId);
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medications for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<IEnumerable<MedicationResponseDto>>> GetActiveByUserId(Guid userId)
        {
            try
            {
                var medications = await _medicationService.GetActiveByUserIdAsync(userId);
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active medications for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<MedicationResponseDto>> GetByIdWithDetails(Guid id)
        {
            try
            {
                var medication = await _medicationService.GetByIdWithDetailsAsync(id);
                if (medication == null)
                    return NotFound($"Medication with ID {id} not found");

                return Ok(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting medication details {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MedicationResponseDto>>> SearchByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Search name cannot be empty");

                var medications = await _medicationService.SearchByNameAsync(name);
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching medications by name {Name}", name);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MedicationResponseDto>> Create([FromBody] MedicationCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var medication = await _medicationService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = medication.Id }, medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medication");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MedicationResponseDto>> Update(Guid id, [FromBody] MedicationUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var medication = await _medicationService.UpdateAsync(id, updateDto);
                if (medication == null)
                    return NotFound($"Medication with ID {id} not found");

                return Ok(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medication {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _medicationService.DeleteAsync(id);
                if (!result)
                    return NotFound($"Medication with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medication {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("search-by-name")]
            public async Task<ActionResult<IEnumerable<MedicationResponseDto>>> Search([FromQuery] string query)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(query))
                        return BadRequest("Search query cannot be empty");

                    var medications = await _medicationService.SearchMedicationsAsync(query);
                    return Ok(medications);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching medications with query {Query}", query);
                    return StatusCode(500, "Internal server error");
                }
            }
    }
}

