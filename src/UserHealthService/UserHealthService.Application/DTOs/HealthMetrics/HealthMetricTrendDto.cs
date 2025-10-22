using System;
using System.Collections.Generic;

namespace UserHealthService.Application.DTOs.HealthMetrics
{
    public class HealthMetricTrendDto
    {
        public List<HealthMetricResponseDto> Metrics { get; set; } = new();
        public string Trend { get; set; } = string.Empty;
        public decimal? ChangePercentage { get; set; }
    }
}

