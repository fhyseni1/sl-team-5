// UserHealthService.Application/Interfaces/IDoctorRepository.cs
using System.Numerics;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.DTOs.Doctors
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllAsync();
        Task<Doctor?> GetByIdAsync(int id);
        Task<Doctor> CreateAsync(DoctorCreateDto dto);
        Task<Doctor?> UpdateAsync(int id, DoctorUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}