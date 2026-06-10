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

namespace ChatService.Application.MediatR.UploadChatAvatar
{
    public class UploadChatAvatarCommand : IRequest<ApiResponse<ChatListItemDto>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public IFormFile File { get; set; } = null!;
    }

    public class UploadChatAvatarHandler
        : IRequestHandler<UploadChatAvatarCommand, ApiResponse<ChatListItemDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public UploadChatAvatarHandler(
            IChatRepository chatRepository,
            IChatMediaRepository mediaRepository,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<ChatListItemDto>> Handle(
            UploadChatAvatarCommand request,
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
                return Fail("FORBIDDEN", "Только админ может менять фото группы");

            if (!ChatValidation.TryValidateGalleryImage(request.File, out var imageCode, out var imageMessage))
                return Fail(imageCode, imageMessage);

            using var stream = request.File.OpenReadStream();

            var upload = await _minio.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                Buckets.ChatImages);

            var media = new ChatMedia
            {
                ChatId = request.ChatId,
                Bucket = upload.Bucket,
                FileKey = upload.FileKey,
                OriginalName = request.File.FileName,
                ContentType = upload.ContentType,
                Size = upload.Size,
                MediaType = "avatar",
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.AddAsync(media, cancellationToken);

            var avatarUrl = await _minio.GetFileUrlAsync(media.FileKey, media.Bucket);

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
                    AvatarUrl = avatarUrl,
                    AvatarIsVideo = false,
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
