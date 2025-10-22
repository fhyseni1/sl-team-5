using UserHealthService.Domain.Entities;

namespace UserHealthService.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Appointment>> GetUpcomingAsync();
        Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime? fromDate, DateTime? toDate);
        Task<Appointment> AddAsync(Appointment appointment);
        Task<Appointment> UpdateAsync(Appointment appointment);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountUpcomingByUserIdAsync(Guid userId);
        Task<List<Appointment>> GetPendingAppointmentsAsync();
        Task<bool> ApproveAsync(Guid id);
        Task<bool> RejectAsync(Guid id);
    }
}