using System;
namespace UserHealthService.Domain.DTOs.Relationships
{
    public class UserRelationshipCreateDto
    {
        public Guid UserId { get; set; }
        public Guid RelatedUserId { get; set; }
        public RelationshipType RelationshipType { get; set; }
        public bool CanManageMedications { get; set; }
        public bool CanViewHealthData { get; set; }
        public bool CanScheduleAppointments { get; set; }
    }
}