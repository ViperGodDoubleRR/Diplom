using ChatService.Domain.Models;

namespace ChatService.Domain.IRepository
{
    public interface IChatRepository
    {
        Task<Chat?> GetByIdAsync(int chatId, CancellationToken cancellationToken = default);

        Task<Chat?> GetPrivateChatBetweenAsync(
            Guid userId1,
            Guid userId2,
            CancellationToken cancellationToken = default);

        Task<List<Chat>> GetUserChatsAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<Chat>> SearchPublicGroupsAsync(
            string search,
            int limit,
            CancellationToken cancellationToken = default);

        Task AddAsync(Chat chat, CancellationToken cancellationToken = default);

        Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default);

        Task<bool> IsMemberAsync(
            int chatId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ChatRole?> GetMemberRoleAsync(
            int chatId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task AddMemberAsync(ChatUser member, CancellationToken cancellationToken = default);

        Task RemoveMemberAsync(
            int chatId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(int chatId, CancellationToken cancellationToken = default);
    }
}
