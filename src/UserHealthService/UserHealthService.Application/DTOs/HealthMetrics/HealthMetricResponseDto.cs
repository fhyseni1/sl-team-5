using System;

namespace UserHealthService.Application.DTOs.HealthMetrics
{
    public class HealthMetricResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public HealthMetricType Type { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public string? Notes { get; set; }
        public string? Device { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TypeDisplay => Type.ToString();
    }
}