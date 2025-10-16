using System;

namespace UserHealthService.Domain.DTOs.UserProfiles
{
    public class UserProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Gender { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? BloodType { get; set; }
        public string? MedicalHistory { get; set; }
        public string? CurrentConditions { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public string? PrimaryCarePhysician { get; set; }
        public string? PrimaryCarePhysicianPhone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal? BMICalculated => CalculateBMI();

        private decimal? CalculateBMI()
        {
            if (!Height.HasValue || !Weight.HasValue || Height <= 0)
                return null;

            var heightInMeters = Height.Value / 100;
            return Math.Round(Weight.Value / (heightInMeters * heightInMeters), 2);
        }
    }
}