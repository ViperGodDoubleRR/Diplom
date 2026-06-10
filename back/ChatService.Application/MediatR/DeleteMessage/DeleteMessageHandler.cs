using ChatService.Application.Abstractions;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace ChatService.Application.MediatR.DeleteMessage
{
    public class DeleteMessageCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int MessageId { get; set; }
    }

    public class DeleteMessageHandler
        : IRequestHandler<DeleteMessageCommand, ApiResponse<bool>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IChatRealtimeNotifier _realtime;

        public DeleteMessageHandler(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            IChatRealtimeNotifier realtime)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _realtime = realtime;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteMessageCommand request,
            CancellationToken cancellationToken)
        {
            var entity = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);

            if (entity is null || entity.IsDeleted)
                return Fail("MESSAGE_NOT_FOUND", "Сообщение не найдено");

            if (!await _chatRepository.IsMemberAsync(entity.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            if (!await CanDeleteMessageAsync(entity, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Можно удалять только свои сообщения");

            entity.IsDeleted = true;
            entity.Text = string.Empty;
            await _messageRepository.UpdateAsync(entity, cancellationToken);

            await _realtime.NotifyMessageDeletedAsync(
                entity.ChatId,
                entity.Id,
                cancellationToken);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }

        private async Task<bool> CanDeleteMessageAsync(
            Domain.Models.Message entity,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (entity.UserId == userId)
                return true;

            var chat = await _chatRepository.GetByIdAsync(entity.ChatId, cancellationToken);
            if (chat is null || chat.Type != ChatType.Group)
                return false;

            var role = await _chatRepository.GetMemberRoleAsync(
                entity.ChatId,
                userId,
                cancellationToken);

            return role == ChatRole.Admin;
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
