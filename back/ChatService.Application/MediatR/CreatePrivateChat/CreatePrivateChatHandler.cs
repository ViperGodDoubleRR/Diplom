using ChatService.Application.DTO;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace ChatService.Application.MediatR.CreatePrivateChat
{
    public class CreatePrivateChatCommand : IRequest<ApiResponse<CreateChatResponse>>
    {
        public Guid UserId { get; set; }
        public Guid TargetUserId { get; set; }
    }

    public class CreatePrivateChatHandler
        : IRequestHandler<CreatePrivateChatCommand, ApiResponse<CreateChatResponse>>
    {
        private readonly IChatRepository _chatRepository;

        public CreatePrivateChatHandler(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ApiResponse<CreateChatResponse>> Handle(
            CreatePrivateChatCommand request,
            CancellationToken cancellationToken)
        {
            if (request.UserId == request.TargetUserId)
            {
                return Fail("INVALID_TARGET", "Нельзя создать чат с самим собой");
            }

            var existing = await _chatRepository.GetPrivateChatBetweenAsync(
                request.UserId,
                request.TargetUserId,
                cancellationToken);

            if (existing is not null)
            {
                return new ApiResponse<CreateChatResponse>
                {
                    Success = true,
                    Data = new CreateChatResponse { ChatId = existing.Id }
                };
            }

            var chat = new Chat
            {
                Type = ChatType.Private,
                IsPublic = false,
                CreatedAt = DateTime.UtcNow
            };

            await _chatRepository.AddAsync(chat, cancellationToken);

            await _chatRepository.AddMemberAsync(new ChatUser
            {
                ChatId = chat.Id,
                UserId = request.UserId,
                Role = ChatRole.Member,
                JoinedAt = DateTime.UtcNow
            }, cancellationToken);

            await _chatRepository.AddMemberAsync(new ChatUser
            {
                ChatId = chat.Id,
                UserId = request.TargetUserId,
                Role = ChatRole.Member,
                JoinedAt = DateTime.UtcNow
            }, cancellationToken);

            return new ApiResponse<CreateChatResponse>
            {
                Success = true,
                Data = new CreateChatResponse { ChatId = chat.Id }
            };
        }

        private static ApiResponse<CreateChatResponse> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
