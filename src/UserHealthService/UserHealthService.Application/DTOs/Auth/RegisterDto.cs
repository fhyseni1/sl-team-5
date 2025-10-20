using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserHealthService.Application.DTOs.Auth
{
    public record RegisterDto(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string PhoneNumber);
}
