using System.Numerics;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.DTOs.Clinics
{
    public interface IClinicRepository
    {
        Task<List<Clinic>> GetAllAsync();
        Task<Clinic?> GetByIdAsync(Guid id);
        Task<Clinic?> GetByAdminIdAsync(Guid adminUserId);
        Task<Clinic> CreateAsync(ClinicCreateDto dto);
        Task<Clinic?> UpdateAsync(Guid id, ClinicUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}