using MedicationService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MedicationService.Application.DTOs.Reminders;

namespace MedicationService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemindersControllercs : ControllerBase
    {
        private readonly IMedicationReminderService _service;
        private readonly ILogger<RemindersControllercs> _logger;
        public RemindersControllercs(IMedicationReminderService service,
        ILogger<RemindersControllercs> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReminderResponseDto>>> GetAll()
        {
            var all= await _service.GetAllAsync();
            return Ok(all);
        }
    }
}
