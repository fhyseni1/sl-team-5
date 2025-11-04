using UserHealthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserHealthService.Application.Interfaces
{
    public interface IAppointmentReportRepository
    {
        Task<AppointmentReport> AddAsync(AppointmentReport report);
        Task<AppointmentReport?> GetByIdAsync(Guid id);
        Task<AppointmentReport?> GetByAppointmentIdAsync(Guid appointmentId);
        Task<IEnumerable<AppointmentReport>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AppointmentReport>> GetByDoctorIdAsync(Guid doctorId);
        Task<AppointmentReport> UpdateAsync(AppointmentReport report);
        Task<bool> DeleteAsync(Guid id);
         Task<IEnumerable<AppointmentReport>> GetAllAsync();
    }
}