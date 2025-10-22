using UserHealthService.Application.DTOs.HealthMetrics;

namespace UserHealthService.Application.DTOs.Users
{
    public class UserDashboardDto
    {
        public int TotalAllergies { get; set; }
        public int UpcomingAppointments { get; set; }
        public int UnreadNotifications { get; set; }
        public HealthMetricResponseDto? LatestHealthMetric { get; set; }
        public DateTime LastLogin { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}