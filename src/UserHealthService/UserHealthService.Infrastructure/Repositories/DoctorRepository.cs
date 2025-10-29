using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.DTOs.Doctors;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserHealthService.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly UserHealthDbContext _context;

        public DoctorRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetAllAsync()
        {
            return await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(Guid id)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        public async Task<Doctor> CreateAsync(DoctorCreateDto dto)
        {
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Specialty = dto.Specialty,
                ClinicName = dto.ClinicName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task<List<Doctor>> GetByClinicIdAsync(Guid clinicId)
        {
            return await _context.Doctors
                .Where(d => d.ClinicId == clinicId && d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Doctor?> UpdateAsync(Guid id, DoctorUpdateDto dto)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null || !doctor.IsActive) return null;
            if (dto.Name != null) doctor.Name = dto.Name;
            if (dto.Specialty != null) doctor.Specialty = dto.Specialty;
            if (dto.ClinicName != null) doctor.ClinicName = dto.ClinicName;
            if (dto.PhoneNumber != null) doctor.PhoneNumber = dto.PhoneNumber;
            if (dto.IsActive.HasValue) doctor.IsActive = dto.IsActive.Value;
            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;
            doctor.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}