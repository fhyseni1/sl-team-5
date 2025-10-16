using System;

namespace UserHealthService.Domain.DTOs.Relationships
{
    public class UserRelationshipUpdateDto
    {
        public RelationshipType RelationshipType { get; set; }
        public bool CanManageMedications { get; set; }
        public bool CanViewHealthData { get; set; }
        public bool CanScheduleAppointments { get; set; }
        public bool IsActive { get; set; }
    }
}