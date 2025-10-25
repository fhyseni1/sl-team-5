using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserHealthDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserHealthDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
            => await _context.Users.Include(u => u.Profile).ToListAsync(ct);

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task<User?> GetWithProfileAsync(Guid id, CancellationToken ct = default)
            => await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken ct = default)
            => await _context.Users.Include(u => u.Profile).Where(u => u.IsActive).ToListAsync(ct);

        public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
            => await _context.Users.AnyAsync(u => u.Email == email, ct);

        public async Task<User> AddAsync(User entity, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(User entity, CancellationToken ct = default)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(User entity, CancellationToken ct = default)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<int> CountAsync(CancellationToken ct = default)
            => await _context.Users.CountAsync(ct);

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => await _context.Users.AnyAsync(u => u.Id == id, ct);

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken ct = default)
            => await _context.Users.Where(predicate).Include(u => u.Profile).ToListAsync(ct);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<int> GetActiveUsersCountAsync(CancellationToken ct = default)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .CountAsync(ct); 
        }

        public async Task<int> GetAllUsersCountAsync(CancellationToken ct = default)
        {
            return await _context.Users.CountAsync(ct);
        }

public async Task<List<DoctorDto>> GetDoctorsAsync(CancellationToken ct = default)
{
    try
    {
        Console.WriteLine("🔍 UserRepository: Getting doctors from database");
        
      
        var doctors = await _context.Users
            .FromSqlRaw(@"SELECT * FROM ""Users"" WHERE ""Type"" IN ('Doctor', 'HealthcareProvider') AND ""IsActive"" = true")
            .Select(u => new DoctorDto
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Specialty = "General Practitioner"
            })
            .ToListAsync(ct);

        Console.WriteLine($"✅ UserRepository: Found {doctors.Count} doctors");
        return doctors;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ UserRepository Error: {ex.Message}");
        throw;
    }
}

public async Task<List<PatientDto>> GetDoctorPatientsAsync(Guid doctorId, CancellationToken ct = default)
{
    try
    {
       
        var doctorExists = await _context.Users
            .FromSqlRaw(@"SELECT * FROM ""Users"" WHERE ""Id"" = {0} AND ""Type"" IN ('Doctor', 'HealthcareProvider')", doctorId)
            .AnyAsync(ct);
        
        if (!doctorExists)
        {
            throw new KeyNotFoundException($"Doctor with ID '{doctorId}' not found");
        }

        var patients = await _context.Users
            .FromSqlRaw(@"SELECT * FROM ""Users"" WHERE ""Type"" = 'Patient' AND ""IsActive"" = true")
            .Select(u => new PatientDto
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                LastAppointment = null,
                TotalAppointments = 0
            })
            .ToListAsync(ct);

        _logger.LogInformation("Retrieved {Count} patients for doctor {DoctorId}", patients.Count, doctorId);
        return patients;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving patients for doctor {DoctorId}", doctorId);
        throw;
    }
}
        public async Task<List<PatientDto>> GetAllPatientsAsync(CancellationToken ct = default)
        {
            try
            {
                var patients = await _context.Users
                    .Where(u => u.Type != Domain.Enums.UserType.HealthcareProvider && u.IsActive)
                    .Select(u => new PatientDto
                    {
                        Id = u.Id,
                        Name = $"{u.FirstName} {u.LastName}",
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                         LastAppointment = null, 
                        TotalAppointments = 0   
                    })
                    .ToListAsync(ct);

                _logger.LogInformation("Retrieved {Count} patients from database", patients.Count);
                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all patients from database");
                throw;
            }
        }
    }
}