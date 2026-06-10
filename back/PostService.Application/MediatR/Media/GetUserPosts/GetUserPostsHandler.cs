using MediatR;

using PostService.Application.DTO;
using PostService.Application.Mapping;
using PostService.Application.Validation;
using PostService.Domain.IRepository;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace PostService.Application.MediatR.Media.GetUserPosts
{
    public class GetUserPostsHandler
        : IRequestHandler<GetUserPostsQuery, ApiResponse<List<PostProfileCard>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetUserPostsHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<PostProfileCard>>> Handle(
            GetUserPostsQuery request,
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
                return new ApiResponse<List<PostProfileCard>>
                {
                    Success = true,
                    Data = []
                };
            }

            var postIds = posts.Select(p => p.Id).ToList();
            var mediaByPost = await _mediaRepository.GetByPostIdsAsync(postIds, cancellationToken);
            var likesCounts = await _postRepository.GetLikesCountsAsync(postIds, cancellationToken);

            var result = new List<PostProfileCard>();

            foreach (var post in posts)
            {
                mediaByPost.TryGetValue(post.Id, out var media);
                var firstMedia = media?.FirstOrDefault();

                result.Add(new PostProfileCard
                {
                    Id = post.Id,
                    Description = post.Description,
                    CreatedAt = post.CreatedAt,
                    MediaUrl = await PostMapper.GetPreviewUrlAsync(firstMedia, _minio, cancellationToken),
                    MediaType = firstMedia?.MediaType,
                    LikesCount = likesCounts.GetValueOrDefault(post.Id)
                });
            }

            return new ApiResponse<List<PostProfileCard>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
