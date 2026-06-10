using ChatService.Application.DTO;
using ChatService.Application.Validation;
using ChatService.Domain.IRepository;
using ChatService.Domain.Models;

using MediatR;

using Shared.Application.Contracts;

namespace ChatService.Application.MediatR.CreateGroupChat
{
    public class CreateGroupChatCommand : IRequest<ApiResponse<CreateChatResponse>>
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
    }

    public class CreateGroupChatHandler
        : IRequestHandler<CreateGroupChatCommand, ApiResponse<CreateChatResponse>>
    {
        private readonly IChatRepository _chatRepository;

        public CreateGroupChatHandler(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ApiResponse<CreateChatResponse>> Handle(
            CreateGroupChatCommand request,
            CancellationToken cancellationToken)
        {
            if (!ChatValidation.TryValidateGroupName(request.Name, out var code, out var message))
                return Fail(code, message);

            var chat = new Chat
            {
                Name = request.Name.Trim(),
                Type = ChatType.Group,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow
            };

            await _chatRepository.AddAsync(chat, cancellationToken);

            await _chatRepository.AddMemberAsync(new ChatUser
            {
                ChatId = chat.Id,
                UserId = request.UserId,
                Role = ChatRole.Admin,
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
