using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.DeleteAllMedia
{
    public class DeleteAllMediaHandler : IRequestHandler<DeleteAllMediaCommand, ApiResponse<bool>>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public DeleteAllMediaHandler(IMediaRepository mediaRepository, IMinioService minio)
        {
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteAllMediaCommand request,
            CancellationToken cancellationToken)
        {
            var mediaList = await _mediaRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            if (mediaList.Count == 0)
            {
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            foreach (var media in mediaList)
            {
                await _minio.DeleteFileAsync(media.FileKey, media.Bucket);
            }

            await _mediaRepository.DeleteRangeAsync(mediaList, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }
    }
}
