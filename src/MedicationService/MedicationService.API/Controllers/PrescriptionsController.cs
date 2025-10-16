
using MedicationService.Application.DTOs.Prescriptions;
using MedicationService.Application.Interfaces;
using MedicationService.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedicationService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<PrescriptionsController> _logger;
        public PrescriptionsController( IPrescriptionService prescriptionService,ILogger<PrescriptionsController> logger)
        {
            _prescriptionService = prescriptionService;
            _logger = logger;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> GetAll()
        {
            try
            {
                var prescriptions = await _prescriptionService.GetAllAsync();
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all prescriptions");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> GetById(Guid id)
        {
            try
            {
                var prescription = await _prescriptionService.GetByIdAsync(id);
                if (prescription == null)
                    return NotFound($"Prescription with ID {id} not found");

                return Ok(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("medication/{medicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> GetByMedication(Guid medicationId)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetByMedicationIdAsync(medicationId);
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions for the medicine Id  {medicationId}", medicationId);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> Create([FromBody]PrescriptionCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var prescription = await _prescriptionService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medication");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> Update(Guid id, [FromBody]PrescriptionUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var prescription = await _prescriptionService.UpdateAsync(id, updateDto);
                if (prescription == null)
                    return NotFound($"Prescription with ID {id} not found");

                return Ok(prescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescreptions  {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> Delete(Guid id, [FromBody] PrescriptionUpdateDto updateDto)
        {
            try
            {
                var result = await _prescriptionService.DeleteAsync(id);
                if (!result)
                    return NotFound($"Prescription with ID {id} not found");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("expiring")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> GetExpiringPrescriptions([FromQuery] DateTime? beforeDate)
        {
            try
            {
                // Use today as default if no date is provided
                var date = beforeDate ?? DateTime.UtcNow;

                var prescriptions = await _prescriptionService.GetExpiringPrescriptionsAsync(date);
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring prescriptions");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("low-refills")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> GetLowRefillPrescriptions([FromQuery] int maxRefills = 2)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetLowRefillPrescriptionsAsync(maxRefills);
                return Ok(prescriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low-refill prescriptions");
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
