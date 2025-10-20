using UserHealthService.Domain.Enums;

namespace UserHealthService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public UserType Type { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual UserProfile? Profile { get; set; }
        public virtual ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<SymptomLog> SymptomLogs { get; set; } = new List<SymptomLog>();
        public virtual ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<UserRelationship> Relationships { get; set; } = new List<UserRelationship>();
    }
}

