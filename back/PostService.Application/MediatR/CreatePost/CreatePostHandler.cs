using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;
using PostService.Domain.IRepository;
using PostService.Domain.Models;

using Shared.Application.Contracts;

namespace PostService.Application.MediatR.CreatePost
{
    public class CreatePostHandler
        : IRequestHandler<CreatePostCommand, ApiResponse<CreatePostResponse>>
    {
        private readonly IPostRepository _postRepository;

        public CreatePostHandler(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<ApiResponse<CreatePostResponse>> Handle(
            CreatePostCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return new ApiResponse<CreatePostResponse>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "DESCRIPTION_REQUIRED",
                        Message = "Description is required"
                    }
                };
            }

            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Description = request.Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _postRepository.AddAsync(post);

            return new ApiResponse<CreatePostResponse>
            {
                Success = true,
                Data = new CreatePostResponse
                {
                    Id = post.Id
                }
            };
        }
    }
}
