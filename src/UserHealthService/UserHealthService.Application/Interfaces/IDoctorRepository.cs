// UserHealthService.Application/Interfaces/IDoctorRepository.cs
using System.Numerics;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.DTOs.Doctors
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllAsync();
        Task<List<Doctor>> GetByClinicIdAsync(Guid clinicId);
        Task<Doctor?> GetByIdAsync(Guid id);
        Task<Doctor> CreateAsync(DoctorCreateDto dto);
        Task<Doctor?> UpdateAsync(Guid id, DoctorUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}