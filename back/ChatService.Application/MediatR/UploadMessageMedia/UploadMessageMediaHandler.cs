using ChatService.Application.Abstractions;
using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Application.Validation;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Microsoft.AspNetCore.Http;

using Shared.Application.Contracts;
using Shared.MinIO.Constants;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.UploadMessageMedia
{
    public class UploadMessageMediaCommand : IRequest<ApiResponse<MessageDto>>
    {
        public Guid UserId { get; set; }
        public int MessageId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string MediaType { get; set; } = "image";
    }

    public class UploadMessageMediaHandler
        : IRequestHandler<UploadMessageMediaCommand, ApiResponse<MessageDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageMediaRepository _mediaRepository;
        private readonly ChatUserResolver _userResolver;
        private readonly IChatRealtimeNotifier _realtime;
        private readonly IMinioService _minio;

        public UploadMessageMediaHandler(
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
            UploadMessageMediaCommand request,
            CancellationToken cancellationToken)
        {
            var message = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);

            if (message is null || message.IsDeleted)
                return Fail("MESSAGE_NOT_FOUND", "Сообщение не найдено");

            if (message.UserId != request.UserId)
                return Fail("FORBIDDEN", "Можно загружать медиа только в свои сообщения");

            if (!await _chatRepository.IsMemberAsync(message.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            if (!ChatValidation.TryValidateMediaUpload(request.File, request.MediaType, out var code, out var error))
                return Fail(code, error);

            var bucket = request.MediaType.Equals("video", StringComparison.OrdinalIgnoreCase)
                ? Buckets.ChatVideos
                : Buckets.ChatImages;

            using var stream = request.File.OpenReadStream();

            var extension = Path.GetExtension(request.File.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = request.MediaType.Equals("video", StringComparison.OrdinalIgnoreCase)
                    ? ".mp4"
                    : ".jpg";
            }

            var fileKey = $"{message.Id}/{Guid.NewGuid():N}{extension}";

            var upload = await _minio.UploadFileAsync(
                stream,
                fileKey,
                request.File.ContentType,
                bucket);

            var media = new MessageMedia
            {
                MessageId = message.Id,
                Bucket = upload.Bucket,
                FileKey = upload.FileKey,
                OriginalName = request.File.FileName,
                ContentType = upload.ContentType,
                Size = upload.Size,
                MediaType = request.MediaType,
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.AddAsync(media, cancellationToken);
            message.Media.Add(media);

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
