using System;

namespace UserHealthService.Application.DTOs.UserProfiles
{
    public class UserProfileUpdateDto
    {
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
    }
}