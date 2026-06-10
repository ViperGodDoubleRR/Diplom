using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Application.Validation;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.UpdateGroupChat
{
    public class UpdateGroupChatCommand : IRequest<ApiResponse<ChatListItemDto>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool? IsPublic { get; set; }
    }

    public class UpdateGroupChatHandler
        : IRequestHandler<UpdateGroupChatCommand, ApiResponse<ChatListItemDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMinioService _minio;

        public UpdateGroupChatHandler(IChatRepository chatRepository, IMinioService minio)
        {
            _chatRepository = chatRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<ChatListItemDto>> Handle(
            UpdateGroupChatCommand request,
            CancellationToken cancellationToken)
        {
            var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);

            if (chat is null || chat.Type != ChatType.Group)
                return Fail("CHAT_NOT_FOUND", "Группа не найдена");

            var role = await _chatRepository.GetMemberRoleAsync(
                request.ChatId,
                request.UserId,
                cancellationToken);

            if (role != ChatRole.Admin)
                return Fail("FORBIDDEN", "Только админ может менять название группы");

            if (!ChatValidation.TryValidateGroupName(request.Name, out var code, out var message))
                return Fail(code, message);

            chat.Name = request.Name.Trim();

            if (request.IsPublic.HasValue)
                chat.IsPublic = request.IsPublic.Value;

            await _chatRepository.UpdateAsync(chat, cancellationToken);

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
                    Name = chat.Name,
                    Type = chat.Type.ToString(),
                    IsPublic = chat.IsPublic,
                    IsMember = true,
                    MyRole = role?.ToString(),
                    AvatarUrl = avatarPreview.Url,
                    AvatarIsVideo = avatarPreview.IsVideo,
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
