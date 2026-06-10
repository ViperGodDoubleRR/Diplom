using MediatR;

using PostService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace PostService.Application.MediatR.DeletePost
{
    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, ApiResponse<bool>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public DeletePostCommandHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeletePostCommand request,
            CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

            if (post is null || post.IsDeleted)
                return Fail("POST_NOT_FOUND", "Пост не найден");

            if (post.UserId != request.UserId)
                return Fail("FORBIDDEN", "Нельзя удалить чужой пост");

            var media = await _mediaRepository.GetByPostIdAsync(post.Id, cancellationToken);

            foreach (var item in media)
            {
                await _minio.DeleteFileAsync(item.FileKey, item.Bucket);
            }

            if (media.Count > 0)
            {
                await _mediaRepository.DeleteRangeAsync(media, cancellationToken);
            }

            post.IsDeleted = true;
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
