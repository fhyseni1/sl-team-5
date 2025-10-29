using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserHealthService.Application.DTOs.Users
{
    public class AssignAssistantDto
    {
        public Guid DoctorId { get; set; }
        public Guid AssistantId { get; set; }
    }
}
