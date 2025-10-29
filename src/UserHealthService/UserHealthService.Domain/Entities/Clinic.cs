using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserHealthService.Domain.Entities
{
    public class Clinic
    {
        public Guid Id { get; set; }
        public string ClinicName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public Guid AdminUserId { get; set; }
    }
}
