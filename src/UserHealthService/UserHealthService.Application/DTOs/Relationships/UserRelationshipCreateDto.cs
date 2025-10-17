using System;
using UserHealthService.Domain.Enums;
namespace UserHealthService.Application.DTOs.Relationships
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