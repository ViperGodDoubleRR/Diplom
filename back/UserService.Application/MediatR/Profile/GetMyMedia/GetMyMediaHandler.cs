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

namespace UserService.Application.MediatR.GetMyMedia
{
    public class GetMyMediaHandler
    : IRequestHandler<GetMyMediaQuery, ApiResponse<List<MediaDto>>>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetMyMediaHandler(
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<MediaDto>>> Handle(
            GetMyMediaQuery request,
            CancellationToken cancellationToken)
        {
            var media = await _mediaRepository
                .GetByUserIdAsync(request.UserId);

            var result = new List<MediaDto>();

            foreach (var m in media)
            {
                var url = await _minio.GetFileUrlAsync(
                    m.FileKey,
                    m.Bucket
                );

                result.Add(new MediaDto
                {
                    Id = m.Id,
                    FileKey = m.FileKey,
                    Bucket = m.Bucket,
                    MediaType = m.MediaType,
                    ContentType = m.ContentType,
                    Url = url
                });
            }

            return new ApiResponse<List<MediaDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
