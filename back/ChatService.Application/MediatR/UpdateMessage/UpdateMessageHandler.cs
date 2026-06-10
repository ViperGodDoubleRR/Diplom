using ChatService.Application.Abstractions;
using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Application.Validation;
using ChatService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.UpdateMessage
{
    public class UpdateMessageCommand : IRequest<ApiResponse<MessageDto>>
    {
        public Guid UserId { get; set; }
        public int MessageId { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class UpdateMessageHandler
        : IRequestHandler<UpdateMessageCommand, ApiResponse<MessageDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ChatUserResolver _userResolver;
        private readonly IChatRealtimeNotifier _realtime;
        private readonly IMinioService _minio;

        public UpdateMessageHandler(
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
            UpdateMessageCommand request,
            CancellationToken cancellationToken)
        {
            var entity = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);

            if (entity is null || entity.IsDeleted)
                return Fail("MESSAGE_NOT_FOUND", "Сообщение не найдено");

            if (entity.UserId != request.UserId)
                return Fail("FORBIDDEN", "Можно редактировать только свои сообщения");

            if (!await _chatRepository.IsMemberAsync(entity.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            if (!ChatValidation.TryValidateMessageText(
                    request.Text,
                    entity.Media.Count > 0,
                    out var code,
                    out var message))
            {
                return Fail(code, message);
            }

            entity.Text = request.Text.Trim();
            await _messageRepository.UpdateAsync(entity, cancellationToken);

            var user = await _userResolver.ResolveAsync(request.UserId, cancellationToken);
            var media = new List<MessageMediaDto>();

            foreach (var item in entity.Media)
            {
                media.Add(await ChatMapper.ToMediaDtoAsync(item, _minio, cancellationToken));
            }

            var dto = ChatMapper.ToMessageDto(entity, user, media);
            await _realtime.NotifyMessageUpdatedAsync(entity.ChatId, dto, cancellationToken);

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
