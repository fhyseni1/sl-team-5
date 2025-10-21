using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Allergies;
using UserHealthService.Application.Interfaces;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllergiesController : ControllerBase
    {
        private readonly IAllergyService _allergyService;

        public AllergiesController(IAllergyService allergyService)
        {
            _allergyService = allergyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AllergyResponseDto>>> GetAll()
        {
            var allergies = await _allergyService.GetAllAsync();
            return Ok(allergies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AllergyResponseDto>> GetById(Guid id)
        {
            var allergy = await _allergyService.GetByIdAsync(id);
            if (allergy == null) return NotFound();
            return Ok(allergy);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AllergyResponseDto>>> GetByUserId(Guid userId)
        {
            var allergies = await _allergyService.GetByUserIdAsync(userId);
            return Ok(allergies);
        }

        [HttpGet("user/{userId}/count")]
        public async Task<ActionResult<int>> GetAllergyCount(Guid userId)
        {
            var count = await _allergyService.GetAllergyCountAsync(userId);
            return Ok(count);
        }

        [HttpPost("check")]
        public async Task<ActionResult> CheckAllergyConflicts([FromBody] AllergyCheckRequest request)
        {
            var result = await _allergyService.CheckAllergyConflictsAsync(request.UserId, request.MedicationName);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<AllergyResponseDto>> Create(AllergyCreateDto createDto)
        {
            var createdAllergy = await _allergyService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdAllergy.Id }, createdAllergy);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AllergyResponseDto>> Update(Guid id, AllergyUpdateDto updateDto)
        {
            var updatedAllergy = await _allergyService.UpdateAsync(id, updateDto);
            if (updatedAllergy == null) return NotFound();
            return Ok(updatedAllergy);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var result = await _allergyService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }

    public class AllergyCheckRequest
    {
        public Guid UserId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
    }
}