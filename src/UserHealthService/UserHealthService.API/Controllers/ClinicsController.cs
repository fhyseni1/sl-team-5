using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Clinics;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClinicsController : ControllerBase
    {
        private readonly IClinicRepository _clinicRepository;
        private readonly ILogger<ClinicsController> _logger;

        public ClinicsController(IClinicRepository clinicRepository, ILogger<ClinicsController> logger)
        {
            _clinicRepository = clinicRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ClinicDto>>> GetAll()
        {
            try
            {
                var clinics = await _clinicRepository.GetAllAsync();
                var clinicDtos = clinics.Select(d => new ClinicDto
                {
                    Id = d.Id,
                    ClinicName = d.ClinicName,
                    Address = d.Address,
                    PhoneNumber = d.PhoneNumber,
                    CreatedAt = d.CreatedAt,
                    IsActive = d.IsActive
                }).ToList();
                return Ok(clinicDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clinics");
                return StatusCode(500, "Error retrieving clinics");
            }
        }

        [HttpGet("admin/{adminId}")]
        public async Task<ActionResult<Clinic>> GetByAdminId(Guid adminId)
        {
            var clinic = await _clinicRepository.GetByAdminIdAsync(adminId);
            if (clinic == null)
            {
                return NotFound(new { message = "Clinic not found for this admin" });
            }
            return Ok(clinic);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Clinic>> GetById(Guid id)
        {
            var clinic = await _clinicRepository.GetByIdAsync(id);
            if (clinic == null)
            {
                return NotFound(new { message = "Clinic not found" });
            }
            return Ok(clinic);
        }

    }
}