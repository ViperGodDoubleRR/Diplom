using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using PostService.Application.DTO;
using PostService.Domain.IRepository;
using PostService.Domain.Models;

using Shared.MinIO.Constants;
using Shared.MinIO.Interfaces;

namespace PostService.Application.MediatR.UpdatePost
{
    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, bool>
    {
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public UpdatePostCommandHandler(
            IPostRepository postRepository,
            IPostMediaRepository mediaRepository,
            IMinioService minio)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<bool> Handle(UpdatePostCommand request, CancellationToken ct)
        {
            var post = await _postRepository.GetByIdAsync(request.PostId);

            if (post == null)
                return false;

            if (post.UserId.ToString() != request.UserId)
                return false;

            post.Description = request.Request.Description?.Trim() ?? "";

            var incoming = request.Request.Media ?? new List<PostMediaDto>();
            var existing = await _mediaRepository.GetByPostIdAsync(post.Id);

            var incomingIds = incoming
                .Where(x => x.Id != Guid.Empty)
                .Select(x => x.Id)
                .ToHashSet();

            var toDelete = existing
                .Where(x => !incomingIds.Contains(x.Id))
                .ToList();

            foreach (var media in toDelete)
            {
                await _minio.DeleteFileAsync(media.FileKey, media.Bucket);
            }

            _mediaRepository.DeleteRange(toDelete);


            var toKeep = existing
                .Where(x => incomingIds.Contains(x.Id))
                .ToList();

            await _postRepository.UpdateAsync(post);

            return true;
        }
    }
}
