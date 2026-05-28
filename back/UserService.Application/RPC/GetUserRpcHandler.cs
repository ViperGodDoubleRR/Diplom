using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetUserPost;

using UserService.Domain.IRepository;
using UserService.Domain.Models;

namespace UserService.Application.RPC
{
    public class GetUserRpcHandler
        : IRPCHandle<GetUserRpcRequest, GetUserRpcResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetUserRpcHandler(
            IUserRepository userRepository,
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _userRepository = userRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<GetUserRpcResponse> Handle(GetUserRpcRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                return new GetUserRpcResponse
                {
                    Id = request.UserId,
                    Login = "",
                    Tag = "",
                    AvatarUrl = ""
                };
            }


            var media = await _mediaRepository.GetByUserIdAsync(user.Id);


            var avatar = media
                .FirstOrDefault(x => x.MediaType == "avatar");

            string avatarUrl = "";

            if (avatar != null)
            {
                avatarUrl = await _minio.GetFileUrlAsync(
                    avatar.FileKey,
                    avatar.Bucket
                );
            }

            return new GetUserRpcResponse
            {
                Id = user.Id,
                Login = user.Login,
                Tag = user.Tag ?? "",
                AvatarUrl = avatarUrl
            };
        }
    }
}
