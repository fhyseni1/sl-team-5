using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly UserHealthDbContext _context;

        public NotificationRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
            => await _context.Notifications.ToListAsync();

        public async Task<Notification?> GetByIdAsync(Guid id)
            => await _context.Notifications.FindAsync(id);

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
            => await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
            => await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

        public async Task AddAsync(Notification notification)
            => await _context.Notifications.AddAsync(notification);

        public async Task UpdateAsync(Notification notification)
            => _context.Notifications.Update(notification);

        public async Task DeleteAsync(Notification notification)
            => _context.Notifications.Remove(notification);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
