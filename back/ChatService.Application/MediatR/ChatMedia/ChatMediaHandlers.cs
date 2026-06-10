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

namespace ChatService.Application.MediatR.ChatGroupMedia
{
    public class GetChatMediaQuery : IRequest<ApiResponse<List<ChatMediaDto>>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
    }

    public class GetChatMediaHandler : IRequestHandler<GetChatMediaQuery, ApiResponse<List<ChatMediaDto>>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public GetChatMediaHandler(
            IChatRepository chatRepository,
            IChatMediaRepository mediaRepository,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<List<ChatMediaDto>>> Handle(
            GetChatMediaQuery request,
            CancellationToken cancellationToken)
        {
            if (!await _chatRepository.IsMemberAsync(request.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            var media = await _mediaRepository.GetByChatIdAsync(request.ChatId, cancellationToken);
            var display = media.Where(m => m.MediaType is "avatar" or "image" or "video").ToList();

            return new ApiResponse<List<ChatMediaDto>>
            {
                Success = true,
                Data = await ChatMediaMapper.ToDtoListAsync(display, _minio, cancellationToken)
            };
        }

        private static ApiResponse<List<ChatMediaDto>> Fail(string code, string message) =>
            new() { Success = false, Error = new ApiError { Code = code, Message = message } };
    }

    public class UploadChatMediaCommand : IRequest<ApiResponse<ChatMediaDto>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string MediaType { get; set; } = "avatar";
    }

    public class UploadChatMediaHandler : IRequestHandler<UploadChatMediaCommand, ApiResponse<ChatMediaDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public UploadChatMediaHandler(
            IChatRepository chatRepository,
            IChatMediaRepository mediaRepository,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<ChatMediaDto>> Handle(
            UploadChatMediaCommand request,
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

            var normalizedType = request.MediaType.Trim().ToLowerInvariant();
            if (normalizedType == "video")
            {
                if (!ChatValidation.TryValidateGalleryVideo(request.File, out var code, out var message))
                    return Fail(code, message);
            }
            else
            {
                if (!ChatValidation.TryValidateGalleryImage(request.File, out var code, out var message))
                    return Fail(code, message);
                normalizedType = "avatar";
            }

            var bucket = normalizedType == "video" ? Buckets.ChatVideos : Buckets.ChatImages;

            using var stream = request.File.OpenReadStream();
            var upload = await _minio.UploadFileAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                bucket);

            var media = new Domain.Models.ChatMedia
            {
                ChatId = request.ChatId,
                Bucket = upload.Bucket,
                FileKey = upload.FileKey,
                OriginalName = request.File.FileName,
                ContentType = upload.ContentType,
                Size = upload.Size,
                MediaType = normalizedType,
                CreatedAt = DateTime.UtcNow
            };

            await _mediaRepository.AddAsync(media, cancellationToken);

            return new ApiResponse<ChatMediaDto>
            {
                Success = true,
                Data = await ChatMediaMapper.ToDtoAsync(media, _minio, cancellationToken)
            };
        }

        private static ApiResponse<ChatMediaDto> Fail(string code, string message) =>
            new() { Success = false, Error = new ApiError { Code = code, Message = message } };
    }

    public class DeleteChatMediaCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public int MediaId { get; set; }
    }

    public class DeleteChatMediaHandler : IRequestHandler<DeleteChatMediaCommand, ApiResponse<bool>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public DeleteChatMediaHandler(
            IChatRepository chatRepository,
            IChatMediaRepository mediaRepository,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteChatMediaCommand request,
            CancellationToken cancellationToken)
        {
            var role = await _chatRepository.GetMemberRoleAsync(
                request.ChatId,
                request.UserId,
                cancellationToken);

            if (role != ChatRole.Admin)
                return Fail("FORBIDDEN", "Только админ может менять фото группы");

            var media = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);

            if (media is null || media.ChatId != request.ChatId)
                return Fail("MEDIA_NOT_FOUND", "Медиа не найдено");

            try
            {
                await _minio.DeleteFileAsync(media.FileKey, media.Bucket);
            }
            catch
            {
                /* ignore */
            }

            await _mediaRepository.DeleteAsync(media, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new() { Success = false, Error = new ApiError { Code = code, Message = message } };
    }

    public class ReplaceChatMediaCommand : IRequest<ApiResponse<ChatMediaDto>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public int MediaId { get; set; }
        public IFormFile File { get; set; } = null!;
        public string MediaType { get; set; } = "avatar";
    }

    public class ReplaceChatMediaHandler : IRequestHandler<ReplaceChatMediaCommand, ApiResponse<ChatMediaDto>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public ReplaceChatMediaHandler(
            IChatRepository chatRepository,
            IChatMediaRepository mediaRepository,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<ChatMediaDto>> Handle(
            ReplaceChatMediaCommand request,
            CancellationToken cancellationToken)
        {
            var role = await _chatRepository.GetMemberRoleAsync(
                request.ChatId,
                request.UserId,
                cancellationToken);

            if (role != ChatRole.Admin)
                return Fail("FORBIDDEN", "Только админ может менять фото группы");

            var existing = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);

            if (existing is null || existing.ChatId != request.ChatId)
                return Fail("MEDIA_NOT_FOUND", "Медиа не найдено");

            try
            {
                await _minio.DeleteFileAsync(existing.FileKey, existing.Bucket);
            }
            catch
            {
                /* ignore */
            }

            await _mediaRepository.DeleteAsync(existing, cancellationToken);

            var uploadHandler = new UploadChatMediaHandler(_chatRepository, _mediaRepository, _minio);
            return await uploadHandler.Handle(new UploadChatMediaCommand
            {
                UserId = request.UserId,
                ChatId = request.ChatId,
                File = request.File,
                MediaType = request.MediaType
            }, cancellationToken);
        }

        private static ApiResponse<ChatMediaDto> Fail(string code, string message) =>
            new() { Success = false, Error = new ApiError { Code = code, Message = message } };
    }

    public class DeleteAllChatMediaCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
    }

    public class DeleteAllChatMediaHandler : IRequestHandler<DeleteAllChatMediaCommand, ApiResponse<bool>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatMediaRepository _mediaRepository;
        private readonly IMinioService _minio;

        public DeleteAllChatMediaHandler(
            IChatRepository chatRepository,
            IChatMediaRepository mediaRepository,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _mediaRepository = mediaRepository;
            _minio = minio;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteAllChatMediaCommand request,
            CancellationToken cancellationToken)
        {
            var role = await _chatRepository.GetMemberRoleAsync(
                request.ChatId,
                request.UserId,
                cancellationToken);

            if (role != ChatRole.Admin)
                return Fail("FORBIDDEN", "Только админ может менять фото группы");

            var media = await _mediaRepository.GetByChatIdAsync(request.ChatId, cancellationToken);

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
            }

            await _mediaRepository.DeleteAllByChatIdAsync(request.ChatId, cancellationToken);

            return new ApiResponse<bool> { Success = true, Data = true };
        }

        private static ApiResponse<bool> Fail(string code, string message) =>
            new() { Success = false, Error = new ApiError { Code = code, Message = message } };
    }
}
