using System;

namespace UserHealthService.Application.DTOs.Relationships
{
    public class UserRelationshipResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid RelatedUserId { get; set; }
        public string RelatedUserName { get; set; } = string.Empty;
        public RelationshipType RelationshipType { get; set; }
        public bool CanManageMedications { get; set; }
        public bool CanViewHealthData { get; set; }
        public bool CanScheduleAppointments { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string RelationshipDisplay => RelationshipType.ToString();
    }
}