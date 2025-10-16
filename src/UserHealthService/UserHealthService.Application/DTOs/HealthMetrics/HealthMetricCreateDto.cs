using System;

namespace UserHealthService.Domain.DTOs.HealthMetrics
{
    public class HealthMetricCreateDto
    {
        public Guid UserId { get; set; }
        public HealthMetricType Type { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public string? Notes { get; set; }
        public string? Device { get; set; }
    }
}