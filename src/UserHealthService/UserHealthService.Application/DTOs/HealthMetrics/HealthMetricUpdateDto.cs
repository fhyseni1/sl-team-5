using System;
using UserHealthService.Domain.Enums;
namespace UserHealthService.Application.DTOs.HealthMetrics
{
    public class HealthMetricUpdateDto
    {
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public string? Notes { get; set; }
        public string? Device { get; set; }
    }
}