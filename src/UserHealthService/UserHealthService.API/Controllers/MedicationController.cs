using Microsoft.AspNetCore.Mvc;
using MedicationService.Protos;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicationController : ControllerBase
    {
        private readonly Medication.MedicationClient _client;

        public MedicationController(Medication.MedicationClient client)
        {
            _client = client;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedication(int id)
        {
            var reply = await _client.GetMedicationInfoAsync(new MedicationRequest { Id = id });
            return Ok(reply);
        }
    }
}
