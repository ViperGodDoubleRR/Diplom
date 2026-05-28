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

namespace UserService.Application.MediatR.GetBlocked
{
    public class GetBlockedHandler
        : IRequestHandler<GetBlockedCommand,
            ApiResponse<List<BlackListDto>>>
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
            var blocked =
                await _socialRepository.GetBlockedAsync(request.UserId);

            var result = new List<BlackListDto>();

            foreach (var user in blocked)
            {
                var avatar =
                    (await _mediaRepository.GetByUserIdAsync(user.BlackId))
                    .FirstOrDefault(x => x.MediaType == "avatar");

                result.Add(new BlackListDto
                {
                    Id = user.Black.Id,
                    Login = user.Black.Login,
                    Tag = user.Black.Tag,
                    AvatarUrl = avatar is null
                        ? null
                        : await _minio.GetFileUrlAsync(
                            avatar.FileKey,
                            avatar.Bucket)
                });
            }

            return new ApiResponse<List<BlackListDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
