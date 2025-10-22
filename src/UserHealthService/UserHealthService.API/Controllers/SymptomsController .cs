using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Symptoms;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Enums;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SymptomsController : ControllerBase
    {
        private readonly ISymptomLogService _symptomLogService;

        public SymptomsController(ISymptomLogService symptomLogService)
        {
            _symptomLogService = symptomLogService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SymptomLogResponseDto>>> GetAll()
        {
            var symptoms = await _symptomLogService.GetAllAsync();
            return Ok(symptoms);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SymptomLogResponseDto>> GetById(Guid id)
        {
            var symptom = await _symptomLogService.GetByIdAsync(id);
            if (symptom == null)
                return NotFound();
            
            return Ok(symptom);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<SymptomLogResponseDto>>> GetByUserId(Guid userId)
        {
            var symptoms = await _symptomLogService.GetByUserIdAsync(userId);
            return Ok(symptoms);
        }

        [HttpGet("user/{userId:guid}/severity/{severity}")]
        public async Task<ActionResult<IEnumerable<SymptomLogResponseDto>>> GetByUserIdAndSeverity(
            Guid userId, SymptomSeverity severity)
        {
            var symptoms = await _symptomLogService.GetByUserIdAndSeverityAsync(userId, severity);
            return Ok(symptoms);
        }

        [HttpGet("severity/{severity}")]
        public async Task<ActionResult<IEnumerable<SymptomLogResponseDto>>> GetSymptomsBySeverity(SymptomSeverity severity)
        {
            var symptoms = await _symptomLogService.GetSymptomsBySeverityAsync(severity);
            return Ok(symptoms);
        }

        [HttpPost]
        public async Task<ActionResult<SymptomLogResponseDto>> Create(SymptomLogCreateDto createDto)
        {
            var symptom = await _symptomLogService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = symptom.Id }, symptom);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SymptomLogResponseDto>> Update(Guid id, SymptomLogUpdateDto updateDto)
        {
            var symptom = await _symptomLogService.UpdateAsync(id, updateDto);
            if (symptom == null)
                return NotFound();
            
            return Ok(symptom);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var result = await _symptomLogService.DeleteAsync(id);
            if (!result)
                return NotFound();
            
            return NoContent();
        }
    }
}