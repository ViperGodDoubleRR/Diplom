using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.GetFriends
{
    public class GetFriendsHandler: IRequestHandler<GetFriendsCommand,ApiResponse<List<FriendDto>>>
    {
        private readonly ISocialRepository _socialRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetFriendsHandler(ISocialRepository socialRepository,IMediaRepository mediaRepository,IMinioService minio)
        {
            _socialRepository = socialRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<FriendDto>>> Handle(GetFriendsCommand request,CancellationToken cancellationToken)
        {
            var friends =
                await _socialRepository.GetFriendsAsync(request.UserId);

            var result = new List<FriendDto>();

            foreach (var friend in friends)
            {
                var avatar =
                    (await _mediaRepository.GetByUserIdAsync(friend.FriendId))
                    .FirstOrDefault(x => x.MediaType == "avatar");

                result.Add(new FriendDto
                {
                    Id = friend.Friend.Id,
                    Login = friend.Friend.Login,
                    Tag = friend.Friend.Tag,
                    AvatarUrl = avatar is null
                        ? null
                        : await _minio.GetFileUrlAsync(
                            avatar.FileKey,
                            avatar.Bucket)
                });
            }

            return new ApiResponse<List<FriendDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
