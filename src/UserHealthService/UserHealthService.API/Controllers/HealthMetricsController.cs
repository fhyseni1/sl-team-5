using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.HealthMetrics;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Enums; 

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthMetricsController : ControllerBase
    {
        private readonly IHealthMetricService _service;
        private readonly IHealthMetricService _healthMetricService;

        public HealthMetricsController(IHealthMetricService service,IHealthMetricService healthMetricService)
        {
            _service = service;
             _healthMetricService = healthMetricService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            return Ok(await _service.GetByUserIdAsync(userId));
        }

        [HttpGet("user/{userId}/type/{type}")]
        public async Task<IActionResult> GetByType(Guid userId, string type)
        {
            return Ok(await _service.GetByUserAndTypeAsync(userId, type));
        }

        [HttpGet("user/{userId}/trends")]
        public async Task<IActionResult> GetTrends(Guid userId)
        {
            return Ok(await _service.GetUserTrendsAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HealthMetricCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] HealthMetricUpdateDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("user/{userId}/latest")]
        public async Task<IActionResult> GetLatestMetric(Guid userId, [FromQuery] HealthMetricType type)
        {
            var result = await _healthMetricService.GetLatestMetricAsync(userId, type);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("user/{userId}/trend")]
        public async Task<IActionResult> GetMetricTrend(Guid userId, [FromQuery] HealthMetricType type)
        {
            var result = await _service.GetMetricTrendAsync(userId, type);
            return Ok(result);
        }
    }
}
