using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using CommentService.Domain.IRepository;
using CommentService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;


namespace CommentService.Application.MediatR.CreateComment
{
    public class CreateCommentHandler
        : IRequestHandler<CreateCommentCommand, ApiResponse<bool>>
    {
        private readonly ICommentRepository _repo;

        public CreateCommentHandler(ICommentRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<bool>> Handle(
            CreateCommentCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false
                };
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                ParentId = request.ParentId,
                UserId = request.UserId,
                Text = request.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(comment);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
    }
}
