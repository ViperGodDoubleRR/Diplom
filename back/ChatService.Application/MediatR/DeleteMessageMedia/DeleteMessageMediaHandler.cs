using ChatService.Application.Abstractions;
using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.DeleteMessageMedia
{
    public class DeleteMessageMediaCommand : IRequest<ApiResponse<MessageDto>>
    {
        public Guid UserId { get; set; }
        public int MessageId { get; set; }
        public int MediaId { get; set; }
    }

    public class DeleteMessageMediaHandler
        : IRequestHandler<DeleteMessageMediaCommand, ApiResponse<MessageDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageMediaRepository _mediaRepository;
        private readonly ChatUserResolver _userResolver;
        private readonly IChatRealtimeNotifier _realtime;
        private readonly IMinioService _minio;

        public DeleteMessageMediaHandler(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            IMessageMediaRepository mediaRepository,
            ChatUserResolver userResolver,
            IChatRealtimeNotifier realtime,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _mediaRepository = mediaRepository;
            _userResolver = userResolver;
            _realtime = realtime;
            _minio = minio;
        }

        public async Task<ApiResponse<MessageDto>> Handle(
            DeleteMessageMediaCommand request,
            CancellationToken cancellationToken)
        {
            var message = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);

            if (message is null || message.IsDeleted)
                return Fail("MESSAGE_NOT_FOUND", "Сообщение не найдено");

            if (message.UserId != request.UserId)
                return Fail("FORBIDDEN", "Можно удалять медиа только из своих сообщений");

            if (!await _chatRepository.IsMemberAsync(message.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            var media = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);

            if (media is null || media.MessageId != message.Id)
                return Fail("MEDIA_NOT_FOUND", "Медиа не найдено");

            try
            {
                await _minio.DeleteFileAsync(media.FileKey, media.Bucket);
            }
            catch
            {
                /* file may already be missing */
            }

            await _mediaRepository.DeleteAsync(media, cancellationToken);

            var tracked = message.Media.FirstOrDefault(m => m.Id == media.Id);
            if (tracked is not null)
                message.Media.Remove(tracked);

            var user = await _userResolver.ResolveAsync(request.UserId, cancellationToken);
            var mediaDtos = new List<MessageMediaDto>();

            foreach (var item in message.Media)
            {
                mediaDtos.Add(await ChatMapper.ToMediaDtoAsync(item, _minio, cancellationToken));
            }

            var dto = ChatMapper.ToMessageDto(message, user, mediaDtos);
            await _realtime.NotifyMessageUpdatedAsync(message.ChatId, dto, cancellationToken);

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
