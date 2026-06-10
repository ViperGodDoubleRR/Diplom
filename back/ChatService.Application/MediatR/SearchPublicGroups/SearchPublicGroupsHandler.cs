using ChatService.Application.Constants;
using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Application.Validation;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.SearchPublicGroups
{
    public class SearchPublicGroupsQuery : IRequest<ApiResponse<List<ChatListItemDto>>>
    {
        public string Search { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    public class SearchPublicGroupsHandler
        : IRequestHandler<SearchPublicGroupsQuery, ApiResponse<List<ChatListItemDto>>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMinioService _minio;

        public SearchPublicGroupsHandler(IChatRepository chatRepository, IMinioService minio)
        {
            _chatRepository = chatRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<ChatListItemDto>>> Handle(
            SearchPublicGroupsQuery request,
            CancellationToken cancellationToken)
        {
            var groups = await _chatRepository.SearchPublicGroupsAsync(
                request.Search.Trim(),
                ChatConstants.SearchGroupsLimit,
                cancellationToken);

            var result = new List<ChatListItemDto>();

            foreach (var group in groups)
            {
                var isMember = await _chatRepository.IsMemberAsync(
                    group.Id,
                    request.UserId,
                    cancellationToken);

                var avatarPreview = await ChatMapper.GetChatAvatarPreviewAsync(
                    group,
                    _minio,
                    cancellationToken);

                result.Add(new ChatListItemDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    Type = group.Type.ToString(),
                    IsPublic = group.IsPublic,
                    IsMember = isMember,
                    AvatarUrl = avatarPreview.Url,
                    AvatarIsVideo = avatarPreview.IsVideo,
                    CreatedAt = group.CreatedAt
                });
            }

            return new ApiResponse<List<ChatListItemDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
