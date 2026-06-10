using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Domain.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public ChatType Type { get; set; }

        /// <summary>Открытая группа — видна в поиске, можно вступить без приглашения.</summary>
        public bool IsPublic { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatUser> Users { get; set; } = new List<ChatUser>();
        public ICollection<ChatMedia> Media { get; set; } = new List<ChatMedia>();
    }
}
