using MedicationService.Application.Interfaces;
using MedicationService.Domain.Entities;
using MedicationService.Domain.Enums;
using MedicationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Infrastructure.Repositories
{
    internal class MedicationReminderRepository : IMedicationReminderRepository
    {
        private readonly MedicationDbContext _context;

        public MedicationReminderRepository(MedicationDbContext context)
        {
            _context = context;
        }
        public  async Task<MedicationReminder> AddAsync(MedicationReminder entity)
        {
            var medicationReminder = new MedicationReminder
            {
                Id = Guid.NewGuid(),
                MedicationId = entity.MedicationId,
                ScheduleId = entity.ScheduleId,
                ScheduledTime = entity.ScheduledTime,
                Status = entity.Status,
                SentAt = entity.SentAt,
                AcknowledgedAt = entity.AcknowledgedAt,
                SnoozeCount = entity.SnoozeCount,
                Message = entity.Message,
                NotificationChannel = entity.NotificationChannel,
                CreatedAt = DateTime.UtcNow
            };

            _context.MedicationReminders.Add(medicationReminder);
            await _context.SaveChangesAsync();

            return medicationReminder;
        }

        public async Task<int> CountAsync()
        {
            return await _context.MedicationReminders.CountAsync();
        }

        public async Task DeleteAsync(MedicationReminder entity)
        {
            _context.MedicationReminders.Remove(entity);
            await _context.SaveChangesAsync();

        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.MedicationReminders.AnyAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MedicationReminder>> FindAsync(Expression<Func<MedicationReminder, bool>> predicate)
        {
            return await _context.MedicationReminders.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<MedicationReminder>> GetAllAsync()
        {
            return await _context.MedicationReminders.ToListAsync();
        }

        public async Task<MedicationReminder?> GetByIdAsync(Guid id)
        {
            return await _context.MedicationReminders.FindAsync(id);
        }

        public async Task<IEnumerable<MedicationReminder>> GetByMedicationIdAsync(Guid medicationId)
        {
            var medicationReminders = await _context.MedicationReminders.Include(e => e.Medication).Where(e => e.MedicationId == medicationId).OrderByDescending(e=>e.CreatedAt).ToListAsync();
            return medicationReminders;
        }

        public async Task<IEnumerable<MedicationReminder>> GetByStatusAsync(ReminderStatus status)
        {
            var medicationReminders = await _context.MedicationReminders.Where(e => e.Status == status).OrderByDescending(c => c.CreatedAt).ToListAsync();
            return medicationReminders;
        }

        public async Task<IEnumerable<MedicationReminder>> GetMissedRemindersAsync()
        {
            return await _context.MedicationReminders
                .Include(r => r.Medication)
                .Include(r => r.Schedule)
                .Where(r => r.Status == ReminderStatus.Scheduled && r.ScheduledTime < DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicationReminder>> GetPendingRemindersAsync()
        {
        
            return await _context.MedicationReminders
                .Include(r => r.Medication)
                .Include(r => r.Schedule)
                .Where(r => r.Status == ReminderStatus.Scheduled && r.ScheduledTime >= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicationReminder>> GetUpcomingRemindersAsync(DateTime beforeTime)
        {
          
            return await _context.MedicationReminders
                .Include(r => r.Medication)
                .Include(r => r.Schedule)
                .Where(r => r.Status == ReminderStatus.Scheduled && r.ScheduledTime <= beforeTime)
                .ToListAsync();
        }

        public async Task UpdateAsync(MedicationReminder entity)
        {
            switch (entity.Status)
            {
                case ReminderStatus.Sent:
                    if (!entity.SentAt.HasValue)
                        entity.SentAt = DateTime.UtcNow;
                    break;
                case ReminderStatus.Acknowledged:
                    if (!entity.AcknowledgedAt.HasValue)
                        entity.AcknowledgedAt = DateTime.UtcNow;
                    break;
            }
            _context.MedicationReminders.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
