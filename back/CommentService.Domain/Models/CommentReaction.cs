using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentService.Domain.Models
{

    public class CommentReaction
    {
        public int Id { get; set; }

        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public Guid UserId { get; set; }

        public ReactionType Type { get; set; }
    }
}
