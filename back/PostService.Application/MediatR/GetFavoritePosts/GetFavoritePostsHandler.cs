using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;
using PostService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;
using Shared.RabbitMQ.rpc.Abstraction;
using Shared.RabbitMQ.rpc.Contracts.GetUserPost;

namespace PostService.Application.MediatR.GetFavoritePosts
{
    public class GetFavoritePostsHandler
     : IRequestHandler<GetFavoritePostsQuery, ApiResponse<List<PostReactionCard>>>
    {
        private readonly IPostRepository _repo;
        private readonly IPostMediaRepository _mediaRepo;
        private readonly IMinioService _minio;
        private readonly IRpcClient _rpc;

        public GetFavoritePostsHandler(
            IPostRepository repo,
            IPostMediaRepository mediaRepo,
            IMinioService minio,
            IRpcClient rpc)
        {
            _repo = repo;
            _mediaRepo = mediaRepo;
            _minio = minio;
            _rpc = rpc;
        }

        public async Task<ApiResponse<List<PostReactionCard>>> Handle(
            GetFavoritePostsQuery request,
            CancellationToken cancellationToken)
        {
            var posts = await _repo.GetFavoritePostsAsync(
                request.UserId,
                request.Page,
                request.PageSize
            );

            var result = new List<PostReactionCard>();

            foreach (var post in posts)
            {
                // ===== USER RPC =====
                var user = await _rpc.CallAsync<GetUserRpcRequest, GetUserRpcResponse>(
                    "user.rpc",
                    new GetUserRpcRequest
                    {
                        UserId = post.UserId
                    });

                // ===== MEDIA FROM DB =====
                var media = await _mediaRepo.GetByPostIdAsync(post.Id);
                var firstMedia = media.FirstOrDefault();

                string? mediaUrl = null;
                string? mediaType = null;

                if (firstMedia != null)
                {
                    // MinIO presigned URL
                    mediaUrl = await _minio.GetFileUrlAsync(
                        firstMedia.FileKey,
                        firstMedia.Bucket
                    );

                    mediaType = firstMedia.MediaType;
                }

                result.Add(new PostReactionCard
                {
                    Id = post.Id,
                    Description = post.Description,
                    CreatedAt = post.CreatedAt,

                    MediaUrl = mediaUrl,
                    MediaType = mediaType,

                    LikesCount = post.Likes?.Count ?? 0,
                    FavoritesCount = post.Favorites?.Count ?? 0,

                    IsLiked = post.Likes?.Any(x => x.UserId == request.UserId) ?? false,
                    IsFavorite = true,
                    UserId = post.UserId,
                    UserLogin = user.Login,
                    UserTag = user.Tag,
                    UserAvatar = user.AvatarUrl
                });
            }

            return new ApiResponse<List<PostReactionCard>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
