using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.DTOs.Clinics;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserHealthService.Infrastructure.Repositories
{
    public class ClinicRepository : IClinicRepository
    {
        private readonly UserHealthDbContext _context;

        public ClinicRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<List<Clinic>> GetAllAsync()
        {
            return await _context.Clinics
                .Where(d => d.IsActive)
                .OrderBy(d => d.ClinicName)
                .ToListAsync();
        }

        public async Task<Clinic?> GetByIdAsync(Guid id)
        {
            return await _context.Clinics
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        public async Task<Clinic?> GetByAdminIdAsync(Guid adminUserId)
        {
            return await _context.Clinics
                .FirstOrDefaultAsync(c => c.AdminUserId == adminUserId && c.IsActive);
        }

        public async Task<Clinic> CreateAsync(ClinicCreateDto dto)
        {
            var clinic = new Clinic
            {
                Id = Guid.NewGuid(),
                ClinicName = dto.ClinicName,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();
            return clinic;
        }

        public async Task<Clinic?> UpdateAsync(Guid id, ClinicUpdateDto dto)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null || !clinic.IsActive) return null;
            if (dto.ClinicName != null) clinic.ClinicName = dto.ClinicName;
            if (dto.Address != null) clinic.Address = dto.Address;
            if (dto.PhoneNumber != null) clinic.PhoneNumber = dto.PhoneNumber;
            if (dto.IsActive.HasValue) clinic.IsActive = dto.IsActive.Value;
            await _context.SaveChangesAsync();
            return clinic;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null) return false;
            clinic.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}