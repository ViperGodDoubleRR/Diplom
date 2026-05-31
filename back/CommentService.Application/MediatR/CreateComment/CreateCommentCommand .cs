using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.CreateComment
{
    public class CreateCommentCommand : IRequest<ApiResponse<bool>>
    {
        public Guid PostId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; } = null!;
    }
}
