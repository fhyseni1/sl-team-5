using UserHealthService.Domain.Enums;

namespace UserHealthService.Domain.Entities
{
    public class SymptomLog
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
        
        public virtual User User { get; set; } = null!;
    }
}

