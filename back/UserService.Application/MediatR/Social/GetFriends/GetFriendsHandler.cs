using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.GetFriends
{
    public class GetFriendsHandler : IRequestHandler<GetFriendsCommand, ApiResponse<List<FriendDto>>>
    {
        private readonly ISocialRepository _socialRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetFriendsHandler(
            ISocialRepository socialRepository,
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _socialRepository = socialRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<FriendDto>>> Handle(
            GetFriendsCommand request,
            CancellationToken cancellationToken)
        {
            var friends = await _socialRepository.GetFriendsAsync(request.UserId, cancellationToken);

            if (friends.Count == 0)
            {
                return new ApiResponse<List<FriendDto>>
                {
                    Success = true,
                    Data = []
                };
            }

            var friendIds = friends.Select(f => f.FriendId).ToList();
            var profileMedia = await _mediaRepository.GetProfileMediaByUserIdsAsync(
                friendIds,
                cancellationToken);

            var previews = await MediaMapper.BuildProfilePreviewMapAsync(
                friendIds,
                profileMedia,
                _minio,
                cancellationToken);

            var result = friends.Select(friend =>
            {
                previews.TryGetValue(friend.FriendId, out var preview);
                preview ??= new ProfilePreviewMedia();

                return new FriendDto
                {
                    Id = friend.Friend.Id,
                    Login = !string.IsNullOrWhiteSpace(friend.ChangeLogin)
                        ? friend.ChangeLogin
                        : friend.Friend.Login,
                    Tag = friend.Friend.Tag,
                    AvatarUrl = preview.Url,
                    AvatarIsVideo = preview.IsVideo
                };
            }).ToList();

            return new ApiResponse<List<FriendDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
