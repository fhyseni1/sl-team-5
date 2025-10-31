using Microsoft.EntityFrameworkCore;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;
using UserHealthService.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace UserHealthService.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly UserHealthDbContext _context;
           private readonly ILogger<AppointmentRepository> _logger;
        public AppointmentRepository(UserHealthDbContext context,ILogger<AppointmentRepository> logger)
        {
            _context = context;
            _logger = logger;
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
  
public async Task<List<Appointment>> GetPendingAppointmentsForAssistantAsync(Guid assistantId)
{
    try
    {
  
        var assignedDoctorIds = await _context.DoctorAssistants
            .Where(da => da.AssistantId == assistantId && da.IsActive)
            .Select(da => da.DoctorId)
            .ToListAsync();

        var assignedDoctors = await _context.Users
            .Where(u => assignedDoctorIds.Contains(u.Id))
            .Select(u => new { 
                Id = u.Id, 
                FullName = $"Dr. {u.FirstName} {u.LastName}",
                FirstName = u.FirstName,
                LastName = u.LastName
            })
            .ToListAsync();

        var assignedDoctorNames = assignedDoctors.Select(d => d.FullName).ToList();

        Console.WriteLine($"🔍 Assistant {assistantId} has {assignedDoctorNames.Count} assigned doctors:");
        foreach (var doctor in assignedDoctors)
        {
            Console.WriteLine($"   - {doctor.FullName} (ID: {doctor.Id})");
        }

        var appointments = await _context.Appointments
            .Include(a => a.User)
            .Where(a => a.Status == AppointmentStatus.Pending)
            .Where(a => assignedDoctorNames.Contains(a.DoctorName))
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        Console.WriteLine($"📅 Found {appointments.Count} pending appointments for assistant");
        foreach (var app in appointments)
        {
            Console.WriteLine($"   - {app.DoctorName} for {app.User?.FirstName} on {app.AppointmentDate}");
        }

        return appointments;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in GetPendingAppointmentsForAssistantAsync: {ex.Message}");
        throw;
    }
}
public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(Guid doctorId)
{
    return await _context.Appointments
        .Include(a => a.User)
        .Where(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Approved) 
        .OrderByDescending(a => a.AppointmentDate)
        .ThenBy(a => a.StartTime)
        .ToListAsync();
}
public async Task<List<Appointment>> GetApprovedAppointmentsForAssistantAsync(Guid assistantId)
{
    try
    {
 
        var assignedDoctorIds = await _context.DoctorAssistants
            .Where(da => da.AssistantId == assistantId && da.IsActive)
            .Select(da => da.DoctorId)
            .ToListAsync();

        var assignedDoctorNames = await _context.Users
            .Where(u => assignedDoctorIds.Contains(u.Id))
            .Select(u => $"Dr. {u.FirstName} {u.LastName}")
            .ToListAsync();

        Console.WriteLine($"🔍 Assistant {assistantId} has {assignedDoctorNames.Count} assigned doctors for approved appointments");

        var appointments = await _context.Appointments
            .Include(a => a.User)
            .Where(a => a.Status == AppointmentStatus.Approved)
            .Where(a => assignedDoctorNames.Contains(a.DoctorName))
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        Console.WriteLine($"✅ Found {appointments.Count} approved appointments for assistant");
        return appointments;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in GetApprovedAppointmentsForAssistantAsync: {ex.Message}");
        throw;
    }
}

public async Task<List<Appointment>> GetRejectedAppointmentsAsync()
{
    return await _context.Appointments
        .Include(a => a.User)
        .Where(a => a.Status == AppointmentStatus.Rejected)
        .OrderByDescending(a => a.AppointmentDate)
        .ThenBy(a => a.StartTime)
        .ToListAsync();
}

        public async Task<List<Appointment>> GetApprovedAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.Status == AppointmentStatus.Approved)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        } 
        
        public async Task<bool> AssistantApproveAsync(Guid id, Guid assistantId)
        {
            try
            {

                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == id && a.Status == AppointmentStatus.Pending);

                if (appointment == null)
                {
                    Console.WriteLine($"❌ Appointment {id} not found or not pending");
                    return false;
                }

                var assignedDoctorIds = await _context.DoctorAssistants
                    .Where(da => da.AssistantId == assistantId && da.IsActive)
                    .Select(da => da.DoctorId)
                    .ToListAsync();

                var assignedDoctorNames = await _context.Users
                    .Where(u => assignedDoctorIds.Contains(u.Id))
                    .Select(u => $"Dr. {u.FirstName} {u.LastName}")
                    .ToListAsync();

                if (!assignedDoctorNames.Contains(appointment.DoctorName))
                {
                    Console.WriteLine($"❌ Assistant {assistantId} not authorized to approve appointment for Dr. {appointment.DoctorName}");
                    Console.WriteLine($"   Assigned doctors: {string.Join(", ", assignedDoctorNames)}");
                    return false;
                }

                appointment.Status = AppointmentStatus.Approved;
                appointment.UpdatedAt = DateTime.UtcNow;

                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Appointment {id} approved by assistant {assistantId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AssistantApproveAsync: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> CanAssistantAccessAppointmentAsync(Guid assistantId, Guid appointmentId)
{
    var appointment = await _context.Appointments
        .Include(a => a.User)
        .FirstOrDefaultAsync(a => a.Id == appointmentId);

    if (appointment == null) return false;

    var doctor = await _context.Doctors
        .FirstOrDefaultAsync(d => d.Name.Contains(appointment.DoctorName));

    if (doctor == null) return false;

    var assignment = await _context.DoctorAssistants
        .FirstOrDefaultAsync(da => da.AssistantId == assistantId && 
                                  da.DoctorId == doctor.Id && 
                                  da.IsActive);

    return assignment != null;
}
public async Task<bool> AssistantRejectAsync(Guid id, Guid assistantId, string rejectionReason)
{
    try
    {
      
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == AppointmentStatus.Pending);

        if (appointment == null)
        {
            Console.WriteLine($"❌ Appointment {id} not found or not pending");
            return false;
        }

        var assignedDoctorIds = await _context.DoctorAssistants
            .Where(da => da.AssistantId == assistantId && da.IsActive)
            .Select(da => da.DoctorId)
            .ToListAsync();

        var assignedDoctorNames = await _context.Users
            .Where(u => assignedDoctorIds.Contains(u.Id))
            .Select(u => $"Dr. {u.FirstName} {u.LastName}")
            .ToListAsync();

        if (!assignedDoctorNames.Contains(appointment.DoctorName))
        {
            Console.WriteLine($"❌ Assistant {assistantId} not authorized to reject appointment for Dr. {appointment.DoctorName}");
            Console.WriteLine($"   Assigned doctors: {string.Join(", ", assignedDoctorNames)}");
            return false;
        }

        appointment.Status = AppointmentStatus.Rejected;
        appointment.RejectionReason = rejectionReason;
        appointment.UpdatedAt = DateTime.UtcNow;

        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ Appointment {id} rejected by assistant {assistantId}");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in AssistantRejectAsync: {ex.Message}");
        return false;
    }
}
public async Task<IEnumerable<Appointment>> GetApprovedAppointmentsByDoctorIdAsync(Guid doctorId)
{
 
    var doctor = await _context.Doctors.FindAsync(doctorId);
    if (doctor == null)
        return Enumerable.Empty<Appointment>();

    return await _context.Appointments
        .Where(a => a.DoctorName == doctor.Name && a.Status == AppointmentStatus.Approved)
        .Include(a => a.User)
        .ToListAsync();
}        public async Task<bool> ApproveAsync(Guid id)
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