using CommentService.Domain.IRepository;
using CommentService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace CommentService.Application.MediatR.AddCommentReaction
{
    public class AddCommentReactionHandler
        : IRequestHandler<AddCommentReactionCommand, ApiResponse<bool>>
    {
        private readonly ICommentRepository _repository;

        public AddCommentReactionHandler(ICommentRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<bool>> Handle(
            AddCommentReactionCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Type != ReactionType.Like && request.Type != ReactionType.Dislike)
                return Fail("INVALID_REACTION", "Доступны только лайк и дизлайк");

            var comment = await _repository.GetByIdAsync(request.CommentId, cancellationToken);

            if (comment is null || comment.IsDeleted)
                return Fail("COMMENT_NOT_FOUND", "Комментарий не найден");

            var existing = await _repository.GetReactionAsync(
                request.CommentId,
                request.UserId,
                cancellationToken);

            if (existing is not null && existing.Type == request.Type)
            {
                await _repository.RemoveReactionAsync(existing, cancellationToken);
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            await _repository.UpsertReactionAsync(new CommentReaction
            {
                CommentId = request.CommentId,
                UserId = request.UserId,
                Type = request.Type
            }, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
