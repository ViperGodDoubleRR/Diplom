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
            var posts = await _postRepository.GetUserPostsAsync(
                request.UserId,
                request.Page,
                request.PageSize
            );

            var result = new List<PostProfileCard>();

            foreach (var post in posts)
            {
                var media = await _mediaRepository.GetByPostIdAsync(post.Id);
                var firstMedia = media.FirstOrDefault();

                string? url = null;

                if (firstMedia != null)
                {
                    url = await _minio.GetFileUrlAsync(
                        firstMedia.FileKey,
                        firstMedia.Bucket
                    );
                }

                var likesCount = await _postRepository.GetLikesCountAsync(post.Id);

                result.Add(new PostProfileCard
                {
                    Id = post.Id,
                    Description = post.Description,
                    CreatedAt = post.CreatedAt,
                    MediaUrl = url,
                    MediaType = firstMedia?.MediaType,

                    // NEW
                    LikesCount = likesCount
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
