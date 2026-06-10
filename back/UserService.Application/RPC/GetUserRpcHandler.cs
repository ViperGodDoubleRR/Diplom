using MediatR;

using Microsoft.Extensions.Logging;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetUserPost;

using UserService.Application.Mapping;
using UserService.Domain.IRepository;

namespace UserService.Application.RPC
{
    public class GetUserRpcHandler
        : IRPCHandle<GetUserRpcRequest, GetUserRpcResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;
        private readonly ILogger<GetUserRpcHandler> _logger;

        public GetUserRpcHandler(
            IUserRepository userRepository,
            IMediaRepository mediaRepository,
            IMinioService minio,
            ILogger<GetUserRpcHandler> logger)
        {
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
            _logger = logger;
        }

        public async Task<GetUserRpcResponse> Handle(GetUserRpcRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user is null)
            {
                _logger.LogDebug("RPC get-user: user {UserId} not found", request.UserId);

                return new GetUserRpcResponse
                {
                    Id = request.UserId,
                    Login = string.Empty,
                    Tag = string.Empty,
                    AvatarUrl = string.Empty
                };
            }

            var profileMedia = await _mediaRepository.GetProfileMediaByUserIdsAsync(
                [user.Id],
                CancellationToken.None);

            var previews = await MediaMapper.BuildProfilePreviewMapAsync(
                [user.Id],
                profileMedia,
                _minio,
                CancellationToken.None);

            previews.TryGetValue(user.Id, out var preview);
            preview ??= new DTO.ProfilePreviewMedia();

            return new GetUserRpcResponse
            {
                Id = user.Id,
                Login = user.Login,
                Tag = user.Tag ?? string.Empty,
                AvatarUrl = preview.Url ?? string.Empty,
                AvatarIsVideo = preview.IsVideo
            };
        }
    }
}
