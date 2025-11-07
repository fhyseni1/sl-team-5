using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Allergies;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Enums;
namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllergiesController : ControllerBase
    {
        private readonly IAllergyService _allergyService;
        private readonly ILogger<AllergiesController> _logger;
        public AllergiesController(
            IAllergyService allergyService,
            ILogger<AllergiesController> logger) 
        {
            _allergyService = allergyService;
            _logger = logger;
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

        [HttpGet("user/{userId}/report")]
        public async Task<ActionResult<AllergyReportDto>> GetAllergyReport(Guid userId)
        {
            try
            {
                var allergies = await _allergyService.GetByUserIdAsync(userId);
                
                var report = new AllergyReportDto
                {
                    UserId = userId,
                    TotalAllergies = allergies.Count(),
                    SevereAllergies = allergies.Count(a => a.Severity == AllergySeverity.Severe || 
                                                        a.Severity == AllergySeverity.LifeThreatening),
                    Allergies = allergies.Select(a => new AllergySummaryDto
                    {
                        AllergenName = a.AllergenName,
                        Severity = a.Severity,
                        Symptoms = a.Symptoms,
                        DiagnosedDate = a.DiagnosedDate
                    }).ToList(),
                    GeneratedAt = DateTime.UtcNow
                };
                
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating allergy report for user {UserId}", userId);
                return StatusCode(500, "Error generating allergy report");
            }
        }

        public class AllergyReportDto
        {
            public Guid UserId { get; set; }
            public int TotalAllergies { get; set; }
            public int SevereAllergies { get; set; }
            public List<AllergySummaryDto> Allergies { get; set; } = new();
            public DateTime GeneratedAt { get; set; }
        }

        public class AllergySummaryDto
        {
            public string AllergenName { get; set; } = string.Empty;
            public AllergySeverity Severity { get; set; }
            public string Symptoms { get; set; } = string.Empty;
            public DateTime? DiagnosedDate { get; set; }
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