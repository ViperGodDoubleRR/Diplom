using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CommentService.Domain.Models
{
    public class Comment
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        public Guid? ParentId { get; set; }
        public Comment? Parent { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public Guid UserId { get; set; }

        public string Text { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<CommentMedia> Media { get; set; } = new List<CommentMedia>();
        public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
    }
}

