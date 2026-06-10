using ChatService.Application.Constants;
using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;
using Shared.MinIO.Interfaces;

namespace ChatService.Application.MediatR.GetMessages
{
    public class GetMessagesQuery : IRequest<ApiResponse<GetMessagesResponse>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
        public int? BeforeMessageId { get; set; }
        public int Limit { get; set; } = ChatConstants.MessagePageSize;
    }

    public class GetMessagesHandler
        : IRequestHandler<GetMessagesQuery, ApiResponse<GetMessagesResponse>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ChatUserResolver _userResolver;
        private readonly IMinioService _minio;

        public GetMessagesHandler(
            IChatRepository chatRepository,
            IMessageRepository messageRepository,
            ChatUserResolver userResolver,
            IMinioService minio)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _userResolver = userResolver;
            _minio = minio;
        }

        public async Task<ApiResponse<GetMessagesResponse>> Handle(
            GetMessagesQuery request,
            CancellationToken cancellationToken)
        {
            if (!await _chatRepository.IsMemberAsync(request.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            var limit = Math.Clamp(request.Limit, 1, ChatConstants.MessagePageSize);

            var messages = await _messageRepository.GetMessagesPageAsync(
                request.ChatId,
                request.BeforeMessageId,
                limit + 1,
                cancellationToken);

            var hasMore = messages.Count > limit;

            if (hasMore)
                messages = messages.Take(limit).ToList();

            var users = await _userResolver.ResolveManyAsync(
                messages.Select(m => m.UserId),
                cancellationToken);

            var items = new List<MessageDto>();

            foreach (var message in messages)
            {
                var user = users[message.UserId];
                var media = new List<MessageMediaDto>();

                foreach (var item in message.Media)
                {
                    media.Add(await ChatMapper.ToMediaDtoAsync(item, _minio, cancellationToken));
                }

                items.Add(ChatMapper.ToMessageDto(message, user, media));
            }

            return new ApiResponse<GetMessagesResponse>
            {
                Success = true,
                Data = new GetMessagesResponse
                {
                    Items = items,
                    HasMore = hasMore
                }
            };
        }

        private static ApiResponse<GetMessagesResponse> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
