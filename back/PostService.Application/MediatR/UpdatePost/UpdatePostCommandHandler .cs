using MediatR;

using PostService.Application.DTO;
using PostService.Application.Validation;
using PostService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace PostService.Application.MediatR.UpdatePost
{
    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, ApiResponse<bool>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public UpdatePostCommandHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            UpdatePostCommand request,
            CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

            if (post is null || post.IsDeleted)
                return Fail("POST_NOT_FOUND", "Пост не найден");

            if (post.UserId != request.UserId)
                return Fail("FORBIDDEN", "Нельзя редактировать чужой пост");

            if (!PostValidation.TryValidateDescription(
                    request.Request.Description,
                    out var code,
                    out var message))
            {
                return Fail(code, message);
            }

            post.Description = request.Request.Description.Trim();

            var incoming = request.Request.Media ?? [];
            var existing = await _mediaRepository.GetByPostIdAsync(post.Id, cancellationToken);

            var incomingIds = incoming
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            var toDelete = existing
                .Where(x => !incomingIds.Contains(x.Id))
                .ToList();

            foreach (var media in toDelete)
            {
                await _minio.DeleteFileAsync(media.FileKey, media.Bucket);
            }

            if (toDelete.Count > 0)
            {
                await _mediaRepository.DeleteRangeAsync(toDelete, cancellationToken);
            }

            await _postRepository.UpdateAsync(post, cancellationToken);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
