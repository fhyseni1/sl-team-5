using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
        Task RemoveByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(string token, Guid userId, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}