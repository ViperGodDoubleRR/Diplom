using CommentService.Application.DTO;
using CommentService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.GetPostComments
{
    public class GetPostCommentsHandler : IRequestHandler<GetPostCommentsQuery, ApiResponse<List<CommentDto>>>
    {
        private readonly ICommentRepository _repo;

        public GetPostCommentsHandler(ICommentRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<List<CommentDto>>> Handle(
            GetPostCommentsQuery request,
            CancellationToken cancellationToken)
        {
            var comments = await _repo.GetByPostIdAsync(request.PostId, cancellationToken);

            return new ApiResponse<List<CommentDto>>
            {
                Success = true,
                Data = comments.Select(x => new CommentDto
                {
                    Id = x.Id,
                    PostId = x.PostId,
                    ParentId = x.ParentId,
                    UserId = x.UserId,
                    Text = x.Text,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToList()
            };
        }
    }
}
