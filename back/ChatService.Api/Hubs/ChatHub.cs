using ChatService.Api.Extensions;
using ChatService.Domain.IRepository;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;

        public ChatHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public static string GroupName(int chatId) => $"chat-{chatId}";

        public async Task JoinChat(int chatId)
        {
            var userId = Context.User?.GetUserId();

            if (userId is null)
                throw new HubException("Unauthorized");

            if (!await _chatRepository.IsMemberAsync(chatId, userId.Value))
                throw new HubException("Нет доступа к чату");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(chatId));
        }

        public Task LeaveChat(int chatId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(chatId));
    }
}
