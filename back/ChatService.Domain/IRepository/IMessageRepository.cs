using ChatService.Domain.Models;

namespace ChatService.Domain.IRepository
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(int messageId, CancellationToken cancellationToken = default);

        Task<List<Message>> GetMessagesPageAsync(
            int chatId,
            int? beforeMessageId,
            int limit,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, Message>> GetLastMessagesByChatIdsAsync(
            IReadOnlyCollection<int> chatIds,
            CancellationToken cancellationToken = default);

        Task AddAsync(Message message, CancellationToken cancellationToken = default);

        Task UpdateAsync(Message message, CancellationToken cancellationToken = default);

        Task<List<Message>> GetActiveByChatIdAsync(
            int chatId,
            CancellationToken cancellationToken = default);

        Task SoftDeleteAllByChatIdAsync(
            int chatId,
            CancellationToken cancellationToken = default);
    }
}
