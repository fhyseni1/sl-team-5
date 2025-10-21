using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetWithProfileAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    }
}
