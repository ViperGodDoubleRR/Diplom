using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Models
{
    public class UserSession
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public string RefreshToken { get; set; } = string.Empty;

        public string? TokenFingerprint { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string DeviceInfo { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}
