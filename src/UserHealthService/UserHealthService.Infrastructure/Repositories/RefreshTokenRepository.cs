// UserHealthService.Infrastructure/Repositories/RefreshTokenRepository.cs
using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly UserHealthDbContext _context;

        // ✅ REMOVED JwtOptions dependency - NOT NEEDED
        public RefreshTokenRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);
        }

        public async Task RemoveByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync(ct);

            if (tokens.Any())
            {
                _context.RefreshTokens.RemoveRange(tokens);
            }
        }

        public async Task AddAsync(string token, Guid userId, CancellationToken ct = default)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.AddDays(7),  // Fixed 7 days
                CreatedAt = DateTime.UtcNow
            };

            await _context.RefreshTokens.AddAsync(refreshToken, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}