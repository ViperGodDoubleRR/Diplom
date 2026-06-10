using MediatR;

using PostService.Application.DTO;
using PostService.Application.Mapping;
using PostService.Application.Validation;
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
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;
        private readonly IRpcClient _rpc;

        public GetFavoritePostsHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio,
            IRpcClient rpc)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
            _rpc = rpc;
        }

        public async Task<ApiResponse<List<PostReactionCard>>> Handle(
            GetFavoritePostsQuery request,
            CancellationToken cancellationToken)
        {
            var (page, pageSize) = PostValidation.NormalizePaging(request.Page, request.PageSize);

            var posts = await _postRepository.GetFavoritePostsAsync(
                request.UserId,
                page,
                pageSize,
                cancellationToken);

            if (posts.Count == 0)
            {
                return new ApiResponse<List<PostReactionCard>>
                {
                    Success = true,
                    Data = []
                };
            }

            var postIds = posts.Select(p => p.Id).ToList();
            var mediaByPost = await _mediaRepository.GetByPostIdsAsync(postIds, cancellationToken);
            var likesCounts = await _postRepository.GetLikesCountsAsync(postIds, cancellationToken);
            var favoritesCounts = await _postRepository.GetFavoritesCountsAsync(postIds, cancellationToken);
            var likedPostIds = await _postRepository.GetLikedPostIdsAsync(
                postIds,
                request.UserId,
                cancellationToken);

            var result = new List<PostReactionCard>();

            foreach (var post in posts)
            {
                var user = await _rpc.CallAsync<GetUserRpcRequest, GetUserRpcResponse>(
                    "user.rpc",
                    new GetUserRpcRequest { UserId = post.UserId });

                mediaByPost.TryGetValue(post.Id, out var media);
                var firstMedia = media?.FirstOrDefault();

                result.Add(new PostReactionCard
                {
                    Id = post.Id,
                    Description = post.Description,
                    CreatedAt = post.CreatedAt,
                    MediaUrl = await PostMapper.GetPreviewUrlAsync(firstMedia, _minio, cancellationToken),
                    MediaType = firstMedia?.MediaType,
                    LikesCount = likesCounts.GetValueOrDefault(post.Id),
                    FavoritesCount = favoritesCounts.GetValueOrDefault(post.Id),
                    IsLiked = likedPostIds.Contains(post.Id),
                    IsFavorite = true,
                    UserId = post.UserId,
                    UserLogin = user?.Login ?? "User",
                    UserTag = user?.Tag ?? string.Empty,
                    UserAvatar = user?.AvatarUrl
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
