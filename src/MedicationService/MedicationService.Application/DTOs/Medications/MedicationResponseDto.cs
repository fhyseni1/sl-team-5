using MedicationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Medications
{
    public class MedicationResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string? DoctorId { get; set; }          
        public string? PrescribedBy { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public MedicationType Type { get; set; }
        public decimal Dosage { get; set; }
        public DosageUnit DosageUnit { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public MedicationStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? NDCCode { get; set; }
        public int ActiveSchedulesCount { get; set; }
        public int UpcomingDosesCount { get; set; }
        public List<Guid> ScheduleIds { get; set; } = new();
    }
}