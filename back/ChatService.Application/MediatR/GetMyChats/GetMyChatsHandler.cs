using ChatService.Application.Constants;
using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.GetMyChats
{
    public class GetMyChatsQuery : IRequest<ApiResponse<List<ChatListItemDto>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetMyChatsHandler
        : IRequestHandler<GetMyChatsQuery, ApiResponse<List<ChatListItemDto>>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ChatUserResolver _userResolver;
        private readonly IMinioService _minio;

        public GetMyChatsHandler(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            ChatUserResolver userResolver,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _userResolver = userResolver;
            _minio = minio;
        }

        public async Task<ApiResponse<List<ChatListItemDto>>> Handle(
            GetMyChatsQuery request,
            CancellationToken cancellationToken)
        {
            var chats = await _chatRepository.GetUserChatsAsync(request.UserId, cancellationToken);

            if (chats.Count == 0)
            {
                return new ApiResponse<List<ChatListItemDto>>
                {
                    Success = true,
                    Data = []
                };
            }

            var chatIds = chats.Select(c => c.Id).ToList();
            var lastMessages = await _messageRepository.GetLastMessagesByChatIdsAsync(
                chatIds,
                cancellationToken);

            var userIds = lastMessages.Values
                .Select(m => m.UserId)
                .Concat(
                    chats
                        .Where(c => c.Type == ChatType.Private)
                        .SelectMany(c => c.Users.Select(u => u.UserId)))
                .Distinct()
                .Where(id => id != request.UserId)
                .ToList();

            var users = await _userResolver.ResolveManyAsync(userIds, cancellationToken);

            var result = new List<ChatListItemDto>();

            foreach (var chat in chats)
            {
                lastMessages.TryGetValue(chat.Id, out var lastMessage);
                ChatUserDto? lastUser = null;

                if (lastMessage is not null)
                {
                    users.TryGetValue(lastMessage.UserId, out lastUser);
                    if (lastUser is null)
                        lastUser = await _userResolver.ResolveAsync(lastMessage.UserId, cancellationToken);
                }

                ChatUserDto? companion = null;

                if (chat.Type == ChatType.Private)
                {
                    var companionId = chat.Users
                        .Select(u => u.UserId)
                        .FirstOrDefault(id => id != request.UserId);

                    if (companionId != Guid.Empty)
                    {
                        users.TryGetValue(companionId, out companion);
                        companion ??= await _userResolver.ResolveAsync(companionId, cancellationToken);
                    }
                }

                var myRole = chat.Users
                    .FirstOrDefault(u => u.UserId == request.UserId)
                    ?.Role;

                var avatarPreview = await ChatMapper.GetChatAvatarPreviewAsync(
                    chat,
                    _minio,
                    cancellationToken);

                result.Add(new ChatListItemDto
                {
                    Id = chat.Id,
                    Name = chat.Type == ChatType.Private
                        ? companion?.Login ?? chat.Name
                        : chat.Name,
                    Type = chat.Type.ToString(),
                    IsPublic = chat.IsPublic,
                    IsMember = true,
                    MyRole = myRole?.ToString(),
                    AvatarUrl = avatarPreview.Url,
                    AvatarIsVideo = avatarPreview.IsVideo,
                    Companion = companion,
                    LastMessage = ChatMapper.ToLastMessagePreview(lastMessage, lastUser),
                    CreatedAt = chat.CreatedAt
                });
            }

            result = result
                .OrderByDescending(c => c.LastMessage?.CreatedAt ?? c.CreatedAt)
                .ToList();

            return new ApiResponse<List<ChatListItemDto>>
            {
                Success = true,
                Data = result
            };
        }
    }
}
