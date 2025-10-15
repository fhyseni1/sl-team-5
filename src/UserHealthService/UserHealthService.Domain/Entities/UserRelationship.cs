using UserHealthService.Domain.Enums;

namespace UserHealthService.Domain.Entities
{
    public class UserRelationship
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RelatedUserId { get; set; }
        public RelationshipType RelationshipType { get; set; }
        public bool CanManageMedications { get; set; }
        public bool CanViewHealthData { get; set; }
        public bool CanScheduleAppointments { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual User User { get; set; } = null!;
        public virtual User RelatedUser { get; set; } = null!;
    }
}

