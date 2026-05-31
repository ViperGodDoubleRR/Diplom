using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Domain.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public Guid UserId { get; set; }

        public string Text { get; set; } = null!;

        public int? ReplyToMessageId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public ICollection<MessageMedia> Media { get; set; } = new List<MessageMedia>();
    }
}
