using System;
using System.Collections.Generic;
using System.Text;

namespace AuthService.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; }=string.Empty;
        public string PasswordHash { get; set; }=string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<UserSession> Sessions { get; set; } = new();
        public List<VerificationCode> VerificationCodes { get; set; } = new();
        public List<ResetCode> ResetCodes { get; set; } = new();

    }
}
