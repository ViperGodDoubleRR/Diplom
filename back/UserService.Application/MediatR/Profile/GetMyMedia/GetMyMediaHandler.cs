using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.GetMyMedia
{
    public class GetMyMediaHandler : IRequestHandler<GetMyMediaQuery, ApiResponse<List<MediaDto>>>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetMyMediaHandler(IMediaRepository mediaRepository, IMinioService minio)
        {
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<MediaDto>>> Handle(
            GetMyMediaQuery request,
            CancellationToken cancellationToken)
        {
            var media = await _mediaRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            return new ApiResponse<List<MediaDto>>
            {
                Success = true,
                Data = await MediaMapper.ToDtoListAsync(media, _minio, cancellationToken)
            };
        }
    }
}
