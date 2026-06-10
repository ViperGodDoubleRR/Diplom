using ChatService.Domain.Models;

namespace ChatService.Domain.IRepository
{
    public interface IMessageMediaRepository
    {
        Task<List<MessageMedia>> GetByMessageIdsAsync(
            IReadOnlyCollection<int> messageIds,
            CancellationToken cancellationToken = default);

        Task<List<MessageMedia>> GetByChatIdAsync(
            int chatId,
            CancellationToken cancellationToken = default);

        Task<MessageMedia?> GetByIdAsync(int mediaId, CancellationToken cancellationToken = default);

        Task AddAsync(MessageMedia media, CancellationToken cancellationToken = default);

        Task DeleteAsync(MessageMedia media, CancellationToken cancellationToken = default);
    }
}
