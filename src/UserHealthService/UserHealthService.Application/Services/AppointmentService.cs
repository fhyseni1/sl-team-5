using AutoMapper;
using UserHealthService.Application.DTOs.Appointments;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;

        public AppointmentService(IAppointmentRepository appointmentRepository, IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
        }

        public async Task<AppointmentResponseDto?> GetByIdAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            return appointment == null ? null : _mapper.Map<AppointmentResponseDto>(appointment);
        }
        

        public async Task<IEnumerable<AppointmentResponseDto>> GetAllAsync()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AppointmentResponseDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var appointments = await _appointmentRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<AppointmentResponseDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetUpcomingAsync()
        {
            var appointments = await _appointmentRepository.GetUpcomingAsync();
            return _mapper.Map<IEnumerable<AppointmentResponseDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDateRangeAsync(DateTime? fromDate, DateTime? toDate)
        {
            var appointments = await _appointmentRepository.GetByDateRangeAsync(fromDate, toDate);
            return _mapper.Map<IEnumerable<AppointmentResponseDto>>(appointments);
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
                AppointmentDate = createDto.AppointmentDate,
                StartTime = TimeSpan.Parse(createDto.StartTime),
                EndTime = TimeSpan.Parse(createDto.EndTime),     
                Purpose = createDto.Purpose,
                Notes = createDto.Notes,
                PhoneNumber = createDto.PhoneNumber,
                Status = AppointmentStatus.Pending,
                ReminderSent = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdAppointment = await _appointmentRepository.AddAsync(appointment);
            return _mapper.Map<AppointmentResponseDto>(createdAppointment);
        }

        public async Task<AppointmentResponseDto?> UpdateAsync(Guid id, AppointmentUpdateDto updateDto)
        {
            var existingAppointment = await _appointmentRepository.GetByIdAsync(id);
            if (existingAppointment == null) return null;

            _mapper.Map(updateDto, existingAppointment);
            var updatedAppointment = await _appointmentRepository.UpdateAsync(existingAppointment);
            return _mapper.Map<AppointmentResponseDto>(updatedAppointment);
        }

        public async Task<bool> CancelAsync(Guid id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return false;

            appointment.Status = Domain.Enums.AppointmentStatus.Cancelled;
            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _appointmentRepository.DeleteAsync(id);
        }

        public async Task<int> GetUpcomingCountAsync(Guid userId)
        {
            return await _appointmentRepository.CountUpcomingByUserIdAsync(userId);
        }


    }
}