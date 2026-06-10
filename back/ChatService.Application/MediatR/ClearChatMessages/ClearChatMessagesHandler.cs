using ChatService.Application.Abstractions;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.ClearChatMessages
{
    public class ClearChatMessagesCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
    }

    public class ClearChatMessagesHandler
        : IRequestHandler<ClearChatMessagesCommand, ApiResponse<bool>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageMediaRepository _messageMediaRepository;
        private readonly IChatRealtimeNotifier _realtime;
        private readonly IMinioService _minio;

        public ClearChatMessagesHandler(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            IMessageMediaRepository messageMediaRepository,
            IChatRealtimeNotifier realtime,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _messageMediaRepository = messageMediaRepository;
            _realtime = realtime;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            ClearChatMessagesCommand request,
            CancellationToken cancellationToken)
        {
            var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);

            if (chat is null)
                return Fail("CHAT_NOT_FOUND", "Чат не найден");

            if (!await _chatRepository.IsMemberAsync(request.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            if (chat.Type == ChatType.Group)
            {
                var role = await _chatRepository.GetMemberRoleAsync(
                    request.ChatId,
                    request.UserId,
                    cancellationToken);

                if (role != ChatRole.Admin)
                    return Fail("FORBIDDEN", "Только админ может очистить группу");
            }

            var messages = await _messageRepository.GetActiveByChatIdAsync(
                request.ChatId,
                cancellationToken);

            if (messages.Count > 0)
            {
                var messageIds = messages.Select(m => m.Id).ToList();
                var media = await _messageMediaRepository.GetByMessageIdsAsync(
                    messageIds,
                    cancellationToken);

                foreach (var item in media)
                {
                    try
                    {
                        await _minio.DeleteFileAsync(item.FileKey, item.Bucket);
                    }
                    catch
                    {
                        /* ignore */
                    }

                    await _messageMediaRepository.DeleteAsync(item, cancellationToken);
                }
            }

            await _messageRepository.SoftDeleteAllByChatIdAsync(request.ChatId, cancellationToken);

            foreach (var message in messages)
            {
                await _realtime.NotifyMessageDeletedAsync(
                    request.ChatId,
                    message.Id,
                    cancellationToken);
            }

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
