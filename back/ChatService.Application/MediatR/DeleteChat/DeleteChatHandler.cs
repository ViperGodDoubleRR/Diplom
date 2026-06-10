using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.DeleteChat
{
    public class DeleteChatCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
    }

    public class DeleteChatHandler : IRequestHandler<DeleteChatCommand, ApiResponse<bool>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatMediaRepository _chatMediaRepository;
        private readonly IMessageMediaRepository _messageMediaRepository;
        private readonly IMinioService _minio;

        public DeleteChatHandler(
            IChatRepository chatRepository,
            IChatMediaRepository chatMediaRepository,
            IMessageMediaRepository messageMediaRepository,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _chatMediaRepository = chatMediaRepository;
            _messageMediaRepository = messageMediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteChatCommand request,
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
                    return Fail("FORBIDDEN", "Только админ может удалить группу");
            }

            await DeleteChatFilesAsync(request.ChatId, cancellationToken);
            await _chatRepository.DeleteAsync(request.ChatId, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private async Task DeleteChatFilesAsync(int chatId, CancellationToken cancellationToken)
        {
            var chatMedia = await _chatMediaRepository.GetByChatIdAsync(chatId, cancellationToken);
            foreach (var item in chatMedia)
            {
                try
                {
                    await _minio.DeleteFileAsync(item.FileKey, item.Bucket);
                }
                catch
                {
                    /* ignore */
                }
            }

            var messageMedia = await _messageMediaRepository.GetByChatIdAsync(chatId, cancellationToken);

            foreach (var item in messageMedia)
            {
                try
                {
                    await _minio.DeleteFileAsync(item.FileKey, item.Bucket);
                }
                catch
                {
                    /* ignore */
                }
            }
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
