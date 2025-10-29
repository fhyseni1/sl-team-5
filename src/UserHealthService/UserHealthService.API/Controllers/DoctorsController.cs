using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Doctors;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
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

        [HttpGet("clinic/{clinicId}")]
        public async Task<ActionResult<List<Doctor>>> GetByClinicId(Guid clinicId)
        {
            var doctors = await _doctorRepository.GetByClinicIdAsync(clinicId);
            return Ok(doctors);
        }

        // GET /api/doctors/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetById(Guid id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }
            return Ok(doctor);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDoctor(Guid id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null)
            {
                return NotFound($"Doctor with ID '{id}' not found");
            }
            await _doctorRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}