using UserService.Domain.Models;

namespace UserService.Domain.IRepository
{
    public interface ISocialRepository
    {
        Task<List<FriendList>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<BlackList>> GetBlockedAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddFriendAsync(FriendList friend, CancellationToken cancellationToken = default);
        Task<bool> IsFriendExistsAsync(Guid myId, Guid friendId, CancellationToken cancellationToken = default);
        Task AddBlockAsync(BlackList entity, CancellationToken cancellationToken = default);
        Task<bool> IsBlockedExistsAsync(Guid myId, Guid blackId, CancellationToken cancellationToken = default);
        Task<bool> IsBlockedBetweenAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
        Task<BlackList?> GetBlockAsync(Guid myId, Guid blackId, CancellationToken cancellationToken = default);
        Task RemoveBlockAsync(BlackList entity, CancellationToken cancellationToken = default);
        Task<FriendList?> GetFriendAsync(Guid myId, Guid friendId, CancellationToken cancellationToken = default);
        Task RemoveFriendAsync(FriendList friend, CancellationToken cancellationToken = default);
        Task UpdateFriendAsync(FriendList friend, CancellationToken cancellationToken = default);
        Task<List<User>> SearchUsersAsync(
            string? search,
            int page,
            int pageSize,
            IReadOnlyCollection<Guid> excludeUserIds,
            CancellationToken cancellationToken = default);
    }
}
