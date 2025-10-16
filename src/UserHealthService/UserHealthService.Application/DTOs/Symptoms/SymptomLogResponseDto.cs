using System;

namespace UserHealthService.Domain.DTOs.Symptoms
{
    public class SymptomLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SymptomName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SymptomSeverity Severity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Location { get; set; }
        public string? Trigger { get; set; }
        public string? Treatment { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SeverityDisplay => Severity.ToString();
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
        public bool IsOngoing => !EndTime.HasValue;
    }
}