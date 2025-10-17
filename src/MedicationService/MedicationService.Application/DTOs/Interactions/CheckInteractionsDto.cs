using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicationService.Application.DTOs.Interactions
{
    public class CheckInteractionsDto
    {
        public List<Guid> MedicationIds { get; set; } = new List<Guid>();
    }
}

