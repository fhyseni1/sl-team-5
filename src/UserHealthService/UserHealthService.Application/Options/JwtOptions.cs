using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserHealthService.Application.Options
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public TimeSpan AccessTokenLifetime { get; set; }
        public TimeSpan RefreshTokenLifetime { get; set; }
    }
}
