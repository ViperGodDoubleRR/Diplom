using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentService.Application.DTO
{
    public class CreateCommentRequest
    {
        public Guid PostId { get; set; }

        public Guid? ParentId { get; set; }

        public string Text { get; set; } = null!;
    }
}
