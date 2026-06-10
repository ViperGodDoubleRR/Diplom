using ChatService.Application.Abstractions;
using ChatService.Api.Hubs;

using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.Services
{
    public class SignalRChatRealtimeNotifier : IChatRealtimeNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRChatRealtimeNotifier(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyMessageSentAsync(
            int chatId,
            object payload,
            CancellationToken cancellationToken = default) =>
            _hubContext.Clients
                .Group(ChatHub.GroupName(chatId))
                .SendAsync("messageSent", payload, cancellationToken);

        public Task NotifyMessageUpdatedAsync(
            int chatId,
            object payload,
            CancellationToken cancellationToken = default) =>
            _hubContext.Clients
                .Group(ChatHub.GroupName(chatId))
                .SendAsync("messageUpdated", payload, cancellationToken);

        public Task NotifyMessageDeletedAsync(
            int chatId,
            int messageId,
            CancellationToken cancellationToken = default) =>
            _hubContext.Clients
                .Group(ChatHub.GroupName(chatId))
                .SendAsync("messageDeleted", new { chatId, messageId }, cancellationToken);
    }
}
