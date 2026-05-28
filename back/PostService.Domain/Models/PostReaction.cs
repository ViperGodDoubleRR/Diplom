using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostService.Domain.Models
{
    public class PostReaction
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        public Post Post { get; set; } = null!;

        public Guid UserId { get; set; }

        public ReactionType Type { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
