using ChatService.Application.Abstractions;
using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Application.Validation;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.SendMessage
{
    public class SendMessageCommand : IRequest<ApiResponse<MessageDto>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int? ReplyToMessageId { get; set; }
    }

    public class SendMessageHandler
        : IRequestHandler<SendMessageCommand, ApiResponse<MessageDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ChatUserResolver _userResolver;
        private readonly IChatRealtimeNotifier _realtime;
        private readonly IMinioService _minio;

        public SendMessageHandler(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            ChatUserResolver userResolver,
            IChatRealtimeNotifier realtime,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _userResolver = userResolver;
            _realtime = realtime;
            _minio = minio;
        }

        public async Task<ApiResponse<MessageDto>> Handle(
            SendMessageCommand request,
            CancellationToken cancellationToken)
        {
            if (!await _chatRepository.IsMemberAsync(request.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            if (!ChatValidation.TryValidateMessageText(request.Text, true, out var code, out var message))
                return Fail(code, message);

            if (request.ReplyToMessageId.HasValue)
            {
                var reply = await _messageRepository.GetByIdAsync(
                    request.ReplyToMessageId.Value,
                    cancellationToken);

                if (reply is null || reply.ChatId != request.ChatId || reply.IsDeleted)
                    return Fail("REPLY_NOT_FOUND", "Сообщение для ответа не найдено");
            }

            var entity = new Message
            {
                ChatId = request.ChatId,
                UserId = request.UserId,
                Text = request.Text.Trim(),
                ReplyToMessageId = request.ReplyToMessageId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _messageRepository.AddAsync(entity, cancellationToken);

            var user = await _userResolver.ResolveAsync(request.UserId, cancellationToken);
            var dto = ChatMapper.ToMessageDto(entity, user, []);

            await _realtime.NotifyMessageSentAsync(request.ChatId, dto, cancellationToken);

            return new ApiResponse<MessageDto>
            {
                Success = true,
                Data = dto
            };
        }

        private static ApiResponse<MessageDto> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
