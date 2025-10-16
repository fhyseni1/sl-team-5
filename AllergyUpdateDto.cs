using System;

namespace UserHealthService.Domain.DTOs.Allergies
{
    public class AllergyUpdateDto
    {
        public string AllergenName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AllergySeverity Severity { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public DateTime? DiagnosedDate { get; set; }
        public string? DiagnosedBy { get; set; }
        public bool IsActive { get; set; }
    }
}