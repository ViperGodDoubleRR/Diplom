using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Domain.Models
{
    public class ChatUser
    {
        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public Guid UserId { get; set; }

        public ChatRole Role { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
