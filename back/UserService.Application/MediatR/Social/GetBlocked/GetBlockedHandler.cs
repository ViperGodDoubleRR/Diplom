using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.GetBlocked
{
    public class GetBlockedHandler
        : IRequestHandler<GetBlockedCommand, ApiResponse<List<BlackListDto>>>
    {
        private readonly ISocialRepository _socialRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetBlockedHandler(
            ISocialRepository socialRepository,
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _socialRepository = socialRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<BlackListDto>>> Handle(
            GetBlockedCommand request,
            CancellationToken cancellationToken)
        {
            var blocked = await _socialRepository.GetBlockedAsync(request.UserId, cancellationToken);

            if (blocked.Count == 0)
            {
                return new ApiResponse<List<BlackListDto>>
                {
                    Success = true,
                    Data = []
                };
            }

            var blockedIds = blocked.Select(b => b.BlackId).ToList();
            var profileMedia = await _mediaRepository.GetProfileMediaByUserIdsAsync(
                blockedIds,
                cancellationToken);

            var previews = await MediaMapper.BuildProfilePreviewMapAsync(
                blockedIds,
                profileMedia,
                _minio,
                cancellationToken);

            var result = blocked.Select(entry =>
            {
                previews.TryGetValue(entry.BlackId, out var preview);
                preview ??= new ProfilePreviewMedia();

                return new BlackListDto
                {
                    Id = entry.Black.Id,
                    Login = entry.Black.Login,
                    Tag = entry.Black.Tag,
                    AvatarUrl = preview.Url,
                    AvatarIsVideo = preview.IsVideo
                };
            }).ToList();

            return new ApiResponse<List<BlackListDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
