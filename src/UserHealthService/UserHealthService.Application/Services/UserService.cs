using AutoMapper;
using UserHealthService.Application.DTOs.Users;
using UserHealthService.Application.DTOs.UserProfiles;
using UserHealthService.Application.DTOs.HealthMetrics;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace UserHealthService.Application.Services
{
    public class UserService : IUserService
    {
          private readonly IUserRepository _userRepository;
        private readonly IAllergyRepository _allergyRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHealthMetricRepository _healthMetricRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IAllergyRepository allergyRepository,
            IAppointmentRepository appointmentRepository,
            INotificationRepository notificationRepository,
            IHealthMetricRepository healthMetricRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _allergyRepository = allergyRepository;
            _appointmentRepository = appointmentRepository;
            _notificationRepository = notificationRepository;
            _healthMetricRepository = healthMetricRepository;
            _mapper = mapper;
        }
        public async Task<UserDashboardDto> GetUserDashboardAsync(Guid userId)
        {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
        throw new KeyNotFoundException($"User with ID '{userId}' not found.");
        }

        var dashboard = new UserDashboardDto
        {
        UserId = userId,
        UserName = $"{user.FirstName} {user.LastName}",
        TotalAllergies = 0,
        UpcomingAppointments = 0,
        UnreadNotifications = 0,
        LatestHealthMetric = null,
        LastLogin = user.UpdatedAt
        };
       try
       {
        var allergies = await _allergyRepository.GetByUserIdAsync(userId);
        dashboard.TotalAllergies = allergies.Count();
       }
       catch (Exception ex)
        {
        Console.WriteLine($"Allergies error: {ex.Message}");
        }

      try
      {
        var appointments = await _appointmentRepository.GetUpcomingAsync();
        dashboard.UpcomingAppointments = appointments.Count(a => a.UserId == userId);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Appointments error: {ex.Message}");
       }

       try
       {
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
        dashboard.UnreadNotifications = notifications.Count();
        }
        catch (Exception ex)
        {
        Console.WriteLine($"Notifications error: {ex.Message}");
        }

        try
        {
        var healthMetrics = await _healthMetricRepository.GetByUserIdAsync(userId);
        var latestHealthMetric = healthMetrics.OrderByDescending(m => m.RecordedAt).FirstOrDefault();
        dashboard.LatestHealthMetric = latestHealthMetric != null ? 
            _mapper.Map<HealthMetricResponseDto>(latestHealthMetric) : null;
        }
        catch (Exception ex)
        {
        Console.WriteLine($"Health metrics error: {ex.Message}");
         }

         return dashboard;
        }
         private async Task<HealthMetricResponseDto?> GetLatestHealthMetricAsync(Guid userId)
        {
            var latestMetric = await _healthMetricRepository.GetAllAsync();
            var userLatestMetric = latestMetric
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.RecordedAt)
                .FirstOrDefault();

            if (userLatestMetric == null)
                return null;

            return new HealthMetricResponseDto
            {
                Id = userLatestMetric.Id,
                UserId = userLatestMetric.UserId,
                Type = userLatestMetric.Type,
                Value = userLatestMetric.Value,
                Unit = userLatestMetric.Unit,
                RecordedAt = userLatestMetric.RecordedAt,
                Notes = userLatestMetric.Notes,
                Device = userLatestMetric.Device,
                CreatedAt = userLatestMetric.CreatedAt
            };
        }
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }

        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto userCreateDto)
        {
            if (await _userRepository.EmailExistsAsync(userCreateDto.Email))
            {
                throw new InvalidOperationException($"User with email '{userCreateDto.Email}' already exists.");
            }

            var user = _mapper.Map<User>(userCreateDto);
            user.Id = Guid.NewGuid();
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var createdUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserResponseDto>(createdUser);
        }

        public async Task<UserResponseDto> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found.");
            }

            _mapper.Map(userUpdateDto, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> UpdateUserProfileAsync(Guid userId, UserProfileUpdateDto profileUpdateDto)
        {
            var user = await _userRepository.GetWithProfileAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{userId}' not found.");
            }

            if (user.Profile == null)
            {
                user.Profile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            _mapper.Map(profileUpdateDto, user.Profile);
            user.Profile.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(user);
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }

        public async Task<IEnumerable<UserResponseDto>> GetActiveUsersAsync()
        {
            var users = await _userRepository.GetActiveUsersAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<int> GetActiveUsersCountAsync(CancellationToken ct = default)
        {
            return await _userRepository.GetActiveUsersCountAsync(ct); 
        }

        public async Task<int> GetAllUsersCountAsync(CancellationToken ct = default)
        {
            return await _userRepository.CountAsync(ct);
        }
    }
}