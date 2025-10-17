using System;
using UserHealthService.Domain.Enums;
namespace UserHealthService.Application.DTOs.Symptoms
{
    public class SymptomLogUpdateDto
    {
        public string SymptomName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SymptomSeverity Severity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Location { get; set; }
        public string? Trigger { get; set; }
        public string? Treatment { get; set; }
        public string? Notes { get; set; }
    }
}