using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Notifications;
using UserHealthService.Application.Interfaces;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            try
            {
                var notifications = await _service.GetByUserIdAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error in GetByUser for userId {userId}: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching user notifications.");
            }
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<IActionResult> GetUnread(Guid userId) =>
            Ok(await _service.GetUnreadByUserIdAsync(userId));

        [HttpGet("caregiver/{caregiverId}")]
        public async Task<IActionResult> GetForCaregiver(Guid caregiverId)
        {
            try
            {
                var notifications = await _service.GetByCaregiverIdAsync(caregiverId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetForCaregiver for caregiverId {caregiverId}: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching caregiver notifications.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(NotificationCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var success = await _service.MarkAsReadAsync(id);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
