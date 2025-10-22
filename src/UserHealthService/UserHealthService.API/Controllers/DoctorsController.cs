// UserHealthService.API/Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserHealthService.Application.DTOs.Doctors;
using UserHealthService.Application.Interfaces;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorRepository doctorRepository, ILogger<DoctorsController> logger)
        {
            _doctorRepository = doctorRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<DoctorDto>>> GetAll()
        {
            try
            {
                var doctors = await _doctorRepository.GetAllAsync();
                var doctorDtos = doctors.Select(d => new DoctorDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Specialty = d.Specialty,
                    ClinicName = d.ClinicName,
                    Address = d.Address,
                    PhoneNumber = d.PhoneNumber,
                    CreatedAt = d.CreatedAt,
                    IsActive = d.IsActive
                }).ToList();
                return Ok(doctorDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors");
                return StatusCode(500, "Error retrieving doctors");
            }
        }
    }
}