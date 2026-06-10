using MediatR;

using PostService.Application.DTO;
using PostService.Application.Validation;
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
            if (!PostValidation.TryValidateDescription(
                    request.Description,
                    out var code,
                    out var message))
            {
                return Fail(code, message);
            }

            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Description = request.Description.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _postRepository.AddAsync(post, cancellationToken);

            return new ApiResponse<CreatePostResponse>
            {
                Success = true,
                Data = new CreatePostResponse { Id = post.Id }
            };
        }

        private static ApiResponse<CreatePostResponse> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
