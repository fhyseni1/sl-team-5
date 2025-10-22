using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using UserHealthService.Infrastructure.Data;

namespace UserHealthService.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly UserHealthDbContext _context;

        public AppointmentRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.UserId == userId && a.Status != AppointmentStatus.Cancelled)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.AppointmentDate >= today &&
                           a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.Completed)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                var fromDateUtc = fromDate.Value.Kind == DateTimeKind.Utc ? fromDate.Value : fromDate.Value.ToUniversalTime();
                query = query.Where(a => a.AppointmentDate >= fromDateUtc.Date);
            }

            if (toDate.HasValue)
            {
                var toDateUtc = toDate.Value.Kind == DateTimeKind.Utc ? toDate.Value : toDate.Value.ToUniversalTime();
                query = query.Where(a => a.AppointmentDate <= toDateUtc.Date);
            }

            return await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (appointment.AppointmentDate.Kind == DateTimeKind.Unspecified)
            {
                appointment.AppointmentDate = DateTime.SpecifyKind(appointment.AppointmentDate, DateTimeKind.Utc);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            appointment.UpdatedAt = DateTime.UtcNow;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Appointments.AnyAsync(a => a.Id == id);
        }

        public async Task<int> CountUpcomingByUserIdAsync(Guid userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId &&
                           a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.Completed &&
                           a.AppointmentDate.Date >= DateTime.UtcNow.Date)
                .CountAsync();
        }

        public async Task<List<Appointment>> GetPendingAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.Status == AppointmentStatus.Pending)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<bool> ApproveAsync(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = AppointmentStatus.Approved;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(Guid id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = AppointmentStatus.Rejected;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}