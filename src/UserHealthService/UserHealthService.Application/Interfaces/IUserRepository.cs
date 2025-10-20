using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetWithProfileAsync(Guid userId);
        Task<IEnumerable<User>> GetActiveUsersAsync();
    }
}

