using UserHealthService.Application.DTOs.Appointments;

namespace UserHealthService.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<AppointmentResponseDto>> GetAllAsync();
        Task<IEnumerable<AppointmentResponseDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AppointmentResponseDto>> GetUpcomingAsync();
        Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDateRangeAsync(DateTime? fromDate, DateTime? toDate);
        Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto createDto);
        Task<AppointmentResponseDto?> UpdateAsync(Guid id, AppointmentUpdateDto updateDto);

        Task<bool> CancelAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetUpcomingCountAsync(Guid userId);
    }
}