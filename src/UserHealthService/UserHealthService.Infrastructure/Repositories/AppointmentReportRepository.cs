using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserHealthService.Infrastructure.Repositories
{
    public class AppointmentReportRepository : IAppointmentReportRepository
    {
        private readonly UserHealthDbContext _context;

        public AppointmentReportRepository(UserHealthDbContext context)
        {
            _context = context;
        }

        public async Task<AppointmentReport> AddAsync(AppointmentReport report)
        {
            _context.AppointmentReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<AppointmentReport?> GetByIdAsync(Guid id)
        {
            return await _context.AppointmentReports
                .Include(r => r.Appointment)
                .Include(r => r.User)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<AppointmentReport?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _context.AppointmentReports
                .Include(r => r.Appointment)
                .Include(r => r.User)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }

        public async Task<IEnumerable<AppointmentReport>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AppointmentReports
                .Include(r => r.Appointment)
                .Include(r => r.User)
                .Include(r => r.Doctor)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentReport>> GetByDoctorIdAsync(Guid doctorId)
        {
            return await _context.AppointmentReports
                .Include(r => r.Appointment)
                .Include(r => r.User)
                .Include(r => r.Doctor)
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<AppointmentReport> UpdateAsync(AppointmentReport report)
        {
            _context.AppointmentReports.Update(report);
            await _context.SaveChangesAsync();
            return report;
        }
         public async Task<IEnumerable<AppointmentReport>> GetAllAsync()
        {
            return await _context.AppointmentReports
                .Include(r => r.Appointment)
                .Include(r => r.User)
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }
            public async Task<bool> DeleteAsync(Guid id)
        {
            var report = await _context.AppointmentReports.FindAsync(id);
            if (report == null)
                return false;

            _context.AppointmentReports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}