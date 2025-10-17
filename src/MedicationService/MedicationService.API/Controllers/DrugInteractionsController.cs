using MedicationService.Application.Interfaces;
using MedicationService.Domain.DTOs.Interactions;
using MedicationService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MedicationService.API.Controllers
{
    [ApiController]
    [Route("api/drug-interactions")]
    public class DrugInteractionsController : ControllerBase
    {
        private readonly IDrugInteractionService _interactionService;
        private readonly ILogger<DrugInteractionsController> _logger;

        public DrugInteractionsController(
            IDrugInteractionService interactionService,
            ILogger<DrugInteractionsController> logger)
        {
            _interactionService = interactionService;
            _logger = logger;
        }

        // GET /api/drug-interactions (get all)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InteractionResponseDto>>> GetAll()
        {
            try
            {
                var interactions = await _interactionService.GetAllInteractionsAsync();
                return Ok(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all drug interactions");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/drug-interactions/{id} (get by id)
        [HttpGet("{id}")]
        public async Task<ActionResult<InteractionResponseDto>> GetById(Guid id)
        {
            try
            {
                var interaction = await _interactionService.GetInteractionByIdAsync(id);
                if (interaction == null)
                    return NotFound($"Drug interaction with ID {id} not found");

                return Ok(interaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drug interaction {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/drug-interactions/medication/{medicationId} (get by medication)
        [HttpGet("medication/{medicationId}")]
        public async Task<ActionResult<IEnumerable<InteractionResponseDto>>> GetByMedicationId(Guid medicationId)
        {
            try
            {
                var interactions = await _interactionService.GetInteractionsByMedicationIdAsync(medicationId);
                return Ok(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drug interactions for medication {MedicationId}", medicationId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST /api/drug-interactions/check (check for interactions)
        [HttpPost("check")]
        public async Task<ActionResult<IEnumerable<InteractionResponseDto>>> CheckInteractions([FromBody] CheckInteractionsDto checkDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (checkDto.MedicationIds == null || checkDto.MedicationIds.Count < 2)
                    return BadRequest("At least 2 medication IDs are required to check for interactions");

                var interactions = await _interactionService.CheckInteractionsAsync(checkDto.MedicationIds);
                return Ok(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking drug interactions");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST /api/drug-interactions (create)
        [HttpPost]
        public async Task<ActionResult<InteractionResponseDto>> Create([FromBody] InteractionCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var interaction = await _interactionService.CreateInteractionAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = interaction.Id }, interaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drug interaction");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT /api/drug-interactions/{id} (update)
        [HttpPut("{id}")]
        public async Task<ActionResult<InteractionResponseDto>> Update(Guid id, [FromBody] InteractionUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var interaction = await _interactionService.UpdateInteractionAsync(id, updateDto);
                if (interaction == null)
                    return NotFound($"Drug interaction with ID {id} not found");

                return Ok(interaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drug interaction {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE /api/drug-interactions/{id} (delete)
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _interactionService.DeleteInteractionAsync(id);
                if (!result)
                    return NotFound($"Drug interaction with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting drug interaction {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/drug-interactions/severity/{severity} (get by severity)
        [HttpGet("severity/{severity}")]
        public async Task<ActionResult<IEnumerable<InteractionResponseDto>>> GetBySeverity(InteractionSeverity severity)
        {
            try
            {
                var interactions = await _interactionService.GetInteractionsBySeverityAsync(severity);
                return Ok(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drug interactions by severity {Severity}", severity);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

