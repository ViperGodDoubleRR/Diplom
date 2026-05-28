using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.CreatePost
{
    public class CreatePostCommand
        : IRequest<ApiResponse<CreatePostResponse>>
    {
        public Guid UserId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
