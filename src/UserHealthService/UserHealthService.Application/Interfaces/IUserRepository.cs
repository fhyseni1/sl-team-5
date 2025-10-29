using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IUserRepository
    {
        // Metodat ekzistuese
        Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetWithProfileAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task<User> AddAsync(User entity, CancellationToken ct = default);
        Task UpdateAsync(User entity, CancellationToken ct = default);
        Task DeleteAsync(User entity, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<int> GetActiveUsersCountAsync(CancellationToken ct = default);
        Task<int> GetAllUsersCountAsync(CancellationToken ct = default);

        // Metodat e reja për doktorët dhe pacientët
        Task<List<DoctorPatientDto>> GetDoctorsAsync(CancellationToken ct = default);
        Task<List<PatientDto>> GetDoctorPatientsAsync(Guid  doctorId, CancellationToken ct = default);
        Task<List<PatientDto>> GetAllPatientsAsync(CancellationToken ct = default);
    }
}