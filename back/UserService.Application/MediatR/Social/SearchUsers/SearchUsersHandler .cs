using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

using UserService.Application.DTO;
using UserService.Application.Mapping;
using UserService.Application.Validation;
using UserService.Domain.IRepository;

namespace UserService.Application.MediatR.Social.SearchUsers
{
    public class SearchUsersHandler
        : IRequestHandler<SearchUsersQuery, ApiResponse<List<UserPreviewDto>>>
    {
        private readonly ISocialRepository _socialRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public SearchUsersHandler(
            ISocialRepository socialRepository,
            IMediaRepository mediaRepository,
            IMinioService minio)
        {
            _socialRepository = socialRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<UserPreviewDto>>> Handle(
            SearchUsersQuery request,
            CancellationToken cancellationToken)
        {
            var (page, pageSize) = UserValidation.NormalizePaging(request.Page, request.PageSize);

            var search = string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();

            if (search?.Length > UserValidation.MaxSearchLength)
            {
                return new ApiResponse<List<UserPreviewDto>>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "SEARCH_TOO_LONG",
                        Message = $"Поисковый запрос не должен превышать {UserValidation.MaxSearchLength} символов"
                    }
                };
            }

            var excludeIds = new HashSet<Guid> { request.MyId };

            var friends = await _socialRepository.GetFriendsAsync(request.MyId, cancellationToken);
            foreach (var friend in friends)
                excludeIds.Add(friend.FriendId);

            var blocked = await _socialRepository.GetBlockedAsync(request.MyId, cancellationToken);
            foreach (var block in blocked)
                excludeIds.Add(block.BlackId);

            var users = await _socialRepository.SearchUsersAsync(
                search,
                page,
                pageSize,
                excludeIds,
                cancellationToken);

            var userIds = users.Select(u => u.Id).ToList();
            var profileMedia = userIds.Count == 0
                ? []
                : await _mediaRepository.GetProfileMediaByUserIdsAsync(userIds, cancellationToken);

            var previews = await MediaMapper.BuildProfilePreviewMapAsync(
                userIds,
                profileMedia,
                _minio,
                cancellationToken);

            var result = users.Select(user =>
            {
                previews.TryGetValue(user.Id, out var preview);
                preview ??= new ProfilePreviewMedia();

                return new UserPreviewDto
                {
                    Id = user.Id,
                    Login = user.Login,
                    Tag = user.Tag,
                    AvatarUrl = preview.Url,
                    AvatarIsVideo = preview.IsVideo
                };
            }).ToList();

            return new ApiResponse<List<UserPreviewDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
