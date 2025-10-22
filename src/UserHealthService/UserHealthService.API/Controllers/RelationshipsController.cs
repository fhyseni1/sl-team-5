using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Relationships;
using UserHealthService.Application.Interfaces;

namespace UserHealthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationshipsController : ControllerBase
    {
        private readonly IUserRelationshipService _relationshipService;
        private readonly ILogger<RelationshipsController> _logger;

        public RelationshipsController(IUserRelationshipService relationshipService, ILogger<RelationshipsController> logger)
        {
            _relationshipService = relationshipService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var relationships = await _relationshipService.GetAllAsync();
            return Ok(relationships);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var relationship = await _relationshipService.GetByIdAsync(id);
            if (relationship == null) return NotFound();
            return Ok(relationship);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var relationships = await _relationshipService.GetByUserIdAsync(userId);
            return Ok(relationships);
        }

        [HttpGet("user/{userId:guid}/caregivers")]
        public async Task<IActionResult> GetCaregivers(Guid userId)
        {
            var caregivers = await _relationshipService.GetCaregiversByUserIdAsync(userId);
            return Ok(caregivers);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserRelationshipCreateDto dto)
        {
            var created = await _relationshipService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserRelationshipUpdateDto dto)
        {
            var updated = await _relationshipService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPut("{id:guid}/permissions")]
        public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] string permissions)
        {
            var updated = await _relationshipService.UpdatePermissionsAsync(id, permissions);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _relationshipService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}