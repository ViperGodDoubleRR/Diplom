using ChatService.Domain.Models;

namespace ChatService.Domain.IRepository
{
    public interface IChatMediaRepository
    {
        Task<ChatMedia?> GetAvatarAsync(int chatId, CancellationToken cancellationToken = default);

        Task<ChatMedia?> GetByIdAsync(int mediaId, CancellationToken cancellationToken = default);

        Task<List<ChatMedia>> GetByChatIdAsync(int chatId, CancellationToken cancellationToken = default);

        Task AddAsync(ChatMedia media, CancellationToken cancellationToken = default);

        Task DeleteAsync(ChatMedia media, CancellationToken cancellationToken = default);

        Task DeleteAllByChatIdAsync(int chatId, CancellationToken cancellationToken = default);
    }
}
