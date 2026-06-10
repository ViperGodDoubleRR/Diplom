using CommentService.Application.DTO;
using CommentService.Application.Validation;
using CommentService.Domain.IRepository;
using CommentService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.CreateComment
{
    public class CreateCommentHandler
        : IRequestHandler<CreateCommentCommand, ApiResponse<CreateCommentResponse>>
    {
        private readonly ICommentRepository _repository;

        public CreateCommentHandler(ICommentRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<CreateCommentResponse>> Handle(
            CreateCommentCommand request,
            CancellationToken cancellationToken)
        {
            var text = request.Text?.Trim() ?? string.Empty;

            if (!CommentValidation.TryValidateText(text, out var code, out var message))
                return Fail(code, message);

            if (request.ParentId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(request.ParentId.Value, cancellationToken);

                if (parent is null || parent.IsDeleted || parent.PostId != request.PostId)
                    return Fail("PARENT_NOT_FOUND", "Родительский комментарий не найден");
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                ParentId = request.ParentId,
                UserId = request.UserId,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(comment, cancellationToken);

            return new ApiResponse<CreateCommentResponse>
            {
                Success = true,
                Data = new CreateCommentResponse { Id = comment.Id }
            };
        }

        private static ApiResponse<CreateCommentResponse> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
