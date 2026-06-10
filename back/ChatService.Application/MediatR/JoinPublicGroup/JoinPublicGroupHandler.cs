using ChatService.Application.DTO;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace ChatService.Application.MediatR.JoinPublicGroup
{
    public class JoinPublicGroupCommand : IRequest<ApiResponse<CreateChatResponse>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
    }

    public class JoinPublicGroupHandler
        : IRequestHandler<JoinPublicGroupCommand, ApiResponse<CreateChatResponse>>
    {
        private readonly IChatRepository _chatRepository;

        public JoinPublicGroupHandler(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ApiResponse<CreateChatResponse>> Handle(
            JoinPublicGroupCommand request,
            CancellationToken cancellationToken)
        {
            var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);

            if (chat is null || chat.Type != ChatType.Group || !chat.IsPublic)
                return Fail("CHAT_NOT_FOUND", "Открытая группа не найдена");

            if (await _chatRepository.IsMemberAsync(request.ChatId, request.UserId, cancellationToken))
            {
                return new ApiResponse<CreateChatResponse>
                {
                    Success = true,
                    Data = new CreateChatResponse { ChatId = request.ChatId }
                };
            }

            await _chatRepository.AddMemberAsync(new ChatUser
            {
                ChatId = request.ChatId,
                UserId = request.UserId,
                Role = ChatRole.Member,
                JoinedAt = DateTime.UtcNow
            }, cancellationToken);

            return new ApiResponse<CreateChatResponse>
            {
                Success = true,
                Data = new CreateChatResponse { ChatId = request.ChatId }
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
