namespace ChatService.Application.Abstractions
{
    public interface IChatRealtimeNotifier
    {
        Task NotifyMessageSentAsync(int chatId, object payload, CancellationToken cancellationToken = default);

        Task NotifyMessageUpdatedAsync(int chatId, object payload, CancellationToken cancellationToken = default);

        Task NotifyMessageDeletedAsync(
            int chatId,
            int messageId,
            CancellationToken cancellationToken = default);
    }
}
