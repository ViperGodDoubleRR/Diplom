using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Domain.IRepository;

using Shared.MinIO.Interfaces;

namespace PostService.Application.MediatR.DeletePost
{
    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public DeletePostCommandHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<bool> Handle(
            DeletePostCommand request,
            CancellationToken cancellationToken)
        {
            var post = await _postRepository.GetByIdAsync(request.PostId);

            if (post == null)
                return false;

            if (post.UserId.ToString() != request.UserId)
                return false;

            var media = await _mediaRepository.GetByPostIdAsync(post.Id);

            foreach (var item in media)
            {
                await _minio.DeleteFileAsync(
                    item.FileKey,
                    item.Bucket
                );
            }

            if (media.Count > 0)
            {
                _mediaRepository.DeleteRange(media);
            }

            post.IsDeleted = true;

            await _postRepository.UpdateAsync(post);

            return true;
        }
    }
}
