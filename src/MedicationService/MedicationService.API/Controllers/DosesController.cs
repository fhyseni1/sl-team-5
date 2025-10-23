using MedicationService.Application.DTOs.Doses;
using MedicationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MedicationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DosesController : ControllerBase
    {
        private readonly IMedicationDoseService _service;

        public DosesController(IMedicationDoseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dose = await _service.GetByIdAsync(id);
            if (dose == null) return NotFound();
            return Ok(dose);
        }

        [HttpGet("medication/{medicationId:guid}")]
        public async Task<IActionResult> GetByMedication(Guid medicationId) =>
            Ok(await _service.GetByMedicationIdAsync(medicationId));

        [HttpGet("user/{userId:guid}/today")]
        public async Task<IActionResult> GetToday(Guid userId) =>
            Ok(await _service.GetTodayDosesAsync(userId));

        [HttpGet("user/{userId:guid}/missed")]
        public async Task<IActionResult> GetMissed(Guid userId) =>
            Ok(await _service.GetMissedDosesAsync(userId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoseCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DoseUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [HttpGet("medication/{medicationId}/adherence")] 
        public async Task<IActionResult> GetAdherenceStatsAsync(Guid medicationId)
        {
          decimal adherenceRate=  await _service.CalculateAdherenceRateAsync(medicationId);
            return Ok(adherenceRate);

        }
    }
}
