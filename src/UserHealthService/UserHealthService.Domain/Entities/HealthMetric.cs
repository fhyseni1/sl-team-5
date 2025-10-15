using UserHealthService.Domain.Enums;

namespace UserHealthService.Domain.Entities
{
    public class HealthMetric
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
        
        public virtual User User { get; set; } = null!;
    }
}

