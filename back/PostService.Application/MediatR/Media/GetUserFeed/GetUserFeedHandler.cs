using MediatR;

using PostService.Application.DTO;
using PostService.Application.Mapping;
using PostService.Application.Services;
using PostService.Application.Validation;
using PostService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace PostService.Application.MediatR.Media.GetUserFeed
{
    public class GetUserPostsFeedHandler
        : IRequestHandler<GetUserPostsFeedQuery, ApiResponse<List<PostFullDto>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;
        private readonly PostCommentsCountProvider _commentsCountProvider;

        public GetUserPostsFeedHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio,
            PostCommentsCountProvider commentsCountProvider)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
            _commentsCountProvider = commentsCountProvider;
        }

        public async Task<ApiResponse<List<PostFullDto>>> Handle(
            GetUserPostsFeedQuery request,
            CancellationToken cancellationToken)
        {
            var (page, pageSize) = PostValidation.NormalizePaging(request.Page, request.PageSize);

            var posts = await _postRepository.GetUserPostsAsync(
                request.UserId,
                page,
                pageSize,
                cancellationToken);

            if (posts.Count == 0)
            {
                return new ApiResponse<List<PostFullDto>>
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
                request.CurrentUserId,
                cancellationToken);
            var favoritePostIds = await _postRepository.GetFavoritePostIdsAsync(
                postIds,
                request.CurrentUserId,
                cancellationToken);
            var commentsCounts = await _commentsCountProvider.GetCountsAsync(
                postIds,
                cancellationToken);

            var result = new List<PostFullDto>();

            foreach (var post in posts)
            {
                mediaByPost.TryGetValue(post.Id, out var media);
                var mediaDtos = await PostMapper.ToDtoListAsync(media ?? [], _minio, cancellationToken);

                result.Add(new PostFullDto
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Description = post.Description,
                    CreatedAt = post.CreatedAt,
                    Media = mediaDtos,
                    LikesCount = likesCounts.GetValueOrDefault(post.Id),
                    FavoritesCount = favoritesCounts.GetValueOrDefault(post.Id),
                    CommentsCount = commentsCounts.GetValueOrDefault(post.Id),
                    IsLiked = likedPostIds.Contains(post.Id),
                    IsFavorite = favoritePostIds.Contains(post.Id)
                });
            }

            return new ApiResponse<List<PostFullDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
