using CommentService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace CommentService.Application.MediatR.DeleteComment
{
    public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, ApiResponse<bool>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICommentMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public DeleteCommentHandler(
            ICommentRepository commentRepository,
            ICommentMediaRepository mediaRepository,
            IMinioService minio)
        {
            _commentRepository = commentRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteCommentCommand request,
            CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);

            if (comment is null || comment.IsDeleted)
                return Fail("COMMENT_NOT_FOUND", "Комментарий не найден");

            if (comment.UserId != request.UserId)
                return Fail("FORBIDDEN", "Нельзя удалить чужой комментарий");

            var media = await _mediaRepository.GetByCommentIdAsync(comment.Id, cancellationToken);

            if (media is not null)
            {
                await _minio.DeleteFileAsync(media.FileKey, media.Bucket);
                await _mediaRepository.DeleteAsync(media, cancellationToken);
            }

            comment.IsDeleted = true;
            await _commentRepository.UpdateAsync(comment, cancellationToken);

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
