using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<AppointmentResponseDto?> GetByIdAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            return appointment == null ? null : MapToResponseDto(appointment);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAllAsync()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return appointments.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var appointments = await _appointmentRepository.GetByUserIdAsync(userId);
            return appointments.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetUpcomingAsync()
        {
            var appointments = await _appointmentRepository.GetUpcomingAsync();
            return appointments.Select(MapToResponseDto);
        }

        public async Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto createDto)
        {
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                UserId = createDto.UserId,
                DoctorName = createDto.DoctorName,
                Specialty = createDto.Specialty,
                ClinicName = createDto.ClinicName,
                Address = createDto.Address,
                AppointmentDate = createDto.AppointmentDate.Date,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Purpose = createDto.Purpose,
                Notes = createDto.Notes,
                PhoneNumber = createDto.PhoneNumber,
                Status = AppointmentStatus.Scheduled,
                ReminderSent = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            var created = await _appointmentRepository.AddAsync(appointment);
            return MapToResponseDto(created);
        }

        public async Task<AppointmentResponseDto?> UpdateAsync(Guid id, AppointmentUpdateDto updateDto)
        {
            var existingAppointment = await _appointmentRepository.GetByIdAsync(id);
            if (existingAppointment == null) return null;

            // Update fields
            existingAppointment.DoctorName = updateDto.DoctorName;
            existingAppointment.Specialty = updateDto.Specialty;
            existingAppointment.ClinicName = updateDto.ClinicName;
            existingAppointment.Address = updateDto.Address;
            existingAppointment.AppointmentDate = updateDto.AppointmentDate.Date;
            existingAppointment.StartTime = updateDto.StartTime;
            existingAppointment.EndTime = updateDto.EndTime;
            existingAppointment.Status = updateDto.Status;
            existingAppointment.Purpose = updateDto.Purpose;
            existingAppointment.Notes = updateDto.Notes;
            existingAppointment.PhoneNumber = updateDto.PhoneNumber;
            existingAppointment.UpdatedAt = DateTime.UtcNow;

            var updated = await _appointmentRepository.UpdateAsync(existingAppointment);
            return MapToResponseDto(updated);
        }

        public async Task<bool> CancelAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return false;

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _appointmentRepository.DeleteAsync(id);
        }

        private AppointmentResponseDto MapToResponseDto(Appointment appointment)
        {
            return new AppointmentResponseDto
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                UserName = appointment.User?.FirstName + " " + appointment.User?.LastName,
                DoctorName = appointment.DoctorName,
                Specialty = appointment.Specialty,
                ClinicName = appointment.ClinicName,
                Address = appointment.Address,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                Purpose = appointment.Purpose,
                Notes = appointment.Notes,
                PhoneNumber = appointment.PhoneNumber,
                ReminderSent = appointment.ReminderSent,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }
    }
}