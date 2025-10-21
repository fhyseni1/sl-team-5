// UserHealthService.Infrastructure/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserHealthDbContext _context;
        public UserRepository(UserHealthDbContext context) => _context = context;

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
                .CountAsync(ct); // Added: Count active users
        }

        public async Task<int> CountAsync(CancellationToken ct = default)
            => await _context.Users.CountAsync(ct);

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => await _context.Users.AnyAsync(u => u.Id == id, ct);

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken ct = default)
            => await _context.Users.Where(predicate).Include(u => u.Profile).ToListAsync(ct);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}