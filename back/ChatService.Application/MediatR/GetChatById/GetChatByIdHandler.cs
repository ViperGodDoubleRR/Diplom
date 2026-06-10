using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.GetChatById
{
    public class GetChatByIdQuery : IRequest<ApiResponse<ChatListItemDto>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
    }

    public class GetChatByIdHandler
        : IRequestHandler<GetChatByIdQuery, ApiResponse<ChatListItemDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ChatUserResolver _userResolver;
        private readonly IMinioService _minio;

        public GetChatByIdHandler(
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

        public async Task<ApiResponse<ChatListItemDto>> Handle(
            GetChatByIdQuery request,
            CancellationToken cancellationToken)
        {
            var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);

            if (chat is null)
                return Fail("CHAT_NOT_FOUND", "Чат не найден");

            var isMember = await _chatRepository.IsMemberAsync(
                request.ChatId,
                request.UserId,
                cancellationToken);

            if (!isMember)
            {
                if (chat.Type != ChatType.Group || !chat.IsPublic)
                    return Fail("FORBIDDEN", "Нет доступа к чату");

                var publicPreview = await ChatMapper.GetChatAvatarPreviewAsync(
                    chat,
                    _minio,
                    cancellationToken);

                return new ApiResponse<ChatListItemDto>
                {
                    Success = true,
                    Data = new ChatListItemDto
                    {
                        Id = chat.Id,
                        Name = chat.Name,
                        Type = chat.Type.ToString(),
                        IsPublic = chat.IsPublic,
                        IsMember = false,
                        MyRole = null,
                        AvatarUrl = publicPreview.Url,
                        AvatarIsVideo = publicPreview.IsVideo,
                        Companion = null,
                        LastMessage = null,
                        CreatedAt = chat.CreatedAt
                    }
                };
            }

            var lastMessages = await _messageRepository.GetLastMessagesByChatIdsAsync(
                [chat.Id],
                cancellationToken);

            lastMessages.TryGetValue(chat.Id, out var lastMessage);
            ChatUserDto? lastUser = null;

            if (lastMessage is not null)
                lastUser = await _userResolver.ResolveAsync(lastMessage.UserId, cancellationToken);

            ChatUserDto? companion = null;

            if (chat.Type == ChatType.Private)
            {
                var companionId = chat.Users
                    .Select(u => u.UserId)
                    .FirstOrDefault(id => id != request.UserId);

                if (companionId != Guid.Empty)
                    companion = await _userResolver.ResolveAsync(companionId, cancellationToken);
            }

            var role = await _chatRepository.GetMemberRoleAsync(
                chat.Id,
                request.UserId,
                cancellationToken);

            var avatarPreview = await ChatMapper.GetChatAvatarPreviewAsync(
                chat,
                _minio,
                cancellationToken);

            return new ApiResponse<ChatListItemDto>
            {
                Success = true,
                Data = new ChatListItemDto
                {
                    Id = chat.Id,
                    Name = chat.Type == ChatType.Private
                        ? companion?.Login ?? chat.Name
                        : chat.Name,
                    Type = chat.Type.ToString(),
                    IsPublic = chat.IsPublic,
                    IsMember = true,
                    MyRole = role?.ToString(),
                    AvatarUrl = avatarPreview.Url,
                    AvatarIsVideo = avatarPreview.IsVideo,
                    Companion = companion,
                    LastMessage = ChatMapper.ToLastMessagePreview(lastMessage, lastUser),
                    CreatedAt = chat.CreatedAt
                }
            };
        }

        private static ApiResponse<ChatListItemDto> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
