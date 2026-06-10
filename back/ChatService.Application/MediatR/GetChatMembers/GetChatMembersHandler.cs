using ChatService.Application.DTO;
using ChatService.Application.Mapping;
using ChatService.Domain.IRepository;

using MediatR;

using Shared.Application.Contracts;

namespace ChatService.Application.MediatR.GetChatMembers
{
    public class GetChatMembersQuery : IRequest<ApiResponse<List<ChatUserDto>>>
    {
        public Guid UserId { get; set; }
        public int ChatId { get; set; }
    }

    public class GetChatMembersHandler
        : IRequestHandler<GetChatMembersQuery, ApiResponse<List<ChatUserDto>>>
    {
        private readonly IChatRepository _chatRepository;
        private readonly ChatUserResolver _userResolver;

        public GetChatMembersHandler(IChatRepository chatRepository, ChatUserResolver userResolver)
        {
            _chatRepository = chatRepository;
            _userResolver = userResolver;
        }

        public async Task<ApiResponse<List<ChatUserDto>>> Handle(
            GetChatMembersQuery request,
            CancellationToken cancellationToken)
        {
            if (!await _chatRepository.IsMemberAsync(request.ChatId, request.UserId, cancellationToken))
                return Fail("FORBIDDEN", "Нет доступа к чату");

            var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);

            if (chat is null)
                return Fail("CHAT_NOT_FOUND", "Чат не найден");

            var userIds = chat.Users.Select(u => u.UserId).Distinct().ToList();
            var users = await _userResolver.ResolveManyAsync(userIds, cancellationToken);

            return new ApiResponse<List<ChatUserDto>>
            {
                Success = true,
                Data = userIds
                    .Select(id => users.TryGetValue(id, out var user)
                        ? user
                        : new ChatUserDto { Id = id, Login = "User", Tag = string.Empty, Avatar = string.Empty })
                    .ToList()
            };
        }

        private static ApiResponse<List<ChatUserDto>> Fail(string code, string message) =>
            new()
            {
                Success = false,
                Error = new ApiError { Code = code, Message = message }
            };
    }
}
