using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserHealthService.Application.DTOs.Auth
{
    public record TokenResponseDto(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType = "Bearer");
}
