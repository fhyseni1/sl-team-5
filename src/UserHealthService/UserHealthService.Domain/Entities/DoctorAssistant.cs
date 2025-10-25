using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserHealthService.Domain.Entities
{
    public class DoctorAssistant
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public Guid AssistantId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual User Doctor { get; set; } = null!;
        public virtual User Assistant { get; set; } = null!;
    }
}