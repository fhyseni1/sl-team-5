using System;

namespace UserHealthService.Domain.DTOs.Allergies
{
    public class AllergyResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AllergenName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AllergySeverity Severity { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public DateTime? DiagnosedDate { get; set; }
        public string? DiagnosedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string SeverityDisplay => Severity.ToString();
    }
}