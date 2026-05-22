    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    namespace AuthService.Domain.Models
    {
        public class VerificationCode
        {
            public int Id { get; set; }
            public Guid CodeUserId { get; set; }
            public User UserID { get; set; } = null!;
            public string Code { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
            public bool IsUsed { get; set; }
        }
    }
