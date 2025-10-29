using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserHealthService.Application.DTOs.Clinics
{
    public class ClinicDto
    {
        public Guid Id { get; set; }
        public string? ClinicName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
