using UserHealthService.Application.DTOs.HealthMetrics;
using System;

namespace UserHealthService.Application.DTOs.Users
{
    public class UserDashboardDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalAllergies { get; set; }
        public int UpcomingAppointments { get; set; }
        public int UnreadNotifications { get; set; }
        public HealthMetricResponseDto? LatestHealthMetric { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastHealthCheck { get; set; }
        public bool HasRecentData { get; set; }
    }
}