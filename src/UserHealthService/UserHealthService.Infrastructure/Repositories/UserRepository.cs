// UserHealthService.Infrastructure/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;  // ← ADD THIS
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository  // ← IMPLEMENTS FROM Application.Interfaces
    {
        private readonly UserHealthDbContext _context;

        public UserRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Users.FindAsync(new object[] { id }, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}