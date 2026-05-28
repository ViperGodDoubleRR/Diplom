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

namespace UserService.Application.MediatR.Social.SearchUsers
{
    public class SearchUsersHandler
    : IRequestHandler<SearchUsersQuery, ApiResponse<List<UserPreviewDto>>>
    {
        private readonly ISocialRepository _repo;
        private readonly IMinioService _minio;

        public SearchUsersHandler(ISocialRepository repo, IMinioService minio)
        {
            _repo = repo;
            _minio = minio;
        }

        public async Task<ApiResponse<List<UserPreviewDto>>> Handle(
            SearchUsersQuery request,
            CancellationToken ct)
        {
            var users = await _repo.SearchUsersAsync(
                request.Search,
                request.Page,
                request.PageSize,
                request.MyId
            );

            var result = new List<UserPreviewDto>();

            foreach (var u in users)
            {
                // берем avatar из MediaUsers
                var avatar = u.MediaUsers?
                    .FirstOrDefault(m => m.MediaType == "avatar");

                string? url = null;

                if (avatar != null)
                {
                    url = await _minio.GetFileUrlAsync(
                        avatar.FileKey,
                        avatar.Bucket
                    );
                }

                result.Add(new UserPreviewDto
                {
                    Id = u.Id,
                    Login = u.Login,
                    Tag = u.Tag,
                    AvatarUrl = url
                });
            }

            return new ApiResponse<List<UserPreviewDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
