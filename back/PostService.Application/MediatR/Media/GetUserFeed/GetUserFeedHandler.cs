using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;
using PostService.Application.MediatR.Media.GetUserPosts;
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

        public GetUserPostsFeedHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<PostFullDto>>> Handle(
            GetUserPostsFeedQuery request,
            CancellationToken cancellationToken)
        {
            var posts = await _postRepository.GetUserPostsAsync(
                request.UserId,
                request.Page,
                request.PageSize
            );

            var result = new List<PostFullDto>();

            foreach (var post in posts)
            {
                var media = await _mediaRepository.GetByPostIdAsync(post.Id);

                var mediaDtos = new List<PostMediaDto>();

                foreach (var m in media)
                {
                    var url = await _minio.GetFileUrlAsync(m.FileKey, m.Bucket);

                    mediaDtos.Add(new PostMediaDto
                    {
                        Id = m.Id,
                        Url = url,
                        FileKey = m.FileKey,
                        Bucket = m.Bucket,
                        MediaType = m.MediaType
                    });
                }

                result.Add(new PostFullDto
                {
                    Id = post.Id,
                    UserId = post.UserId,
                    Description = post.Description,
                    CreatedAt = post.CreatedAt,

                    Media = mediaDtos,

                    LikesCount = await _postRepository.GetLikesCountAsync(post.Id),

                    FavoritesCount = await _postRepository.GetFavoritesCountAsync(post.Id),

                    CommentsCount = 0,

                    IsLiked = await _postRepository.IsPostLikedAsync(post.Id,request.CurrentUserId),

                    IsFavorite = await _postRepository.IsPostFavoriteAsync(post.Id,request.CurrentUserId)
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
