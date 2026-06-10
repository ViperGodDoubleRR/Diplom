using Microsoft.EntityFrameworkCore;

using UserService.Domain.IRepository;
using UserService.Domain.Models;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.EfRepository
{
    public class EfSocialRepository : ISocialRepository
    {
        private readonly DbContextUser _context;

        public EfSocialRepository(DbContextUser context)
        {
            _context = context;
        }

        public Task<List<FriendList>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default) =>
            _context.FriendLists
                .Include(x => x.Friend)
                .Where(x => x.MyId == userId)
                .ToListAsync(cancellationToken);

        public Task<List<BlackList>> GetBlockedAsync(Guid userId, CancellationToken cancellationToken = default) =>
            _context.BlackLists
                .Include(x => x.Black)
                .Where(x => x.MyId == userId)
                .ToListAsync(cancellationToken);

        public async Task AddFriendAsync(FriendList friend, CancellationToken cancellationToken = default)
        {
            await _context.FriendLists.AddAsync(friend, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> IsFriendExistsAsync(
            Guid myId,
            Guid friendId,
            CancellationToken cancellationToken = default) =>
            _context.FriendLists.AnyAsync(
                x => x.MyId == myId && x.FriendId == friendId,
                cancellationToken);

        public async Task AddBlockAsync(BlackList entity, CancellationToken cancellationToken = default)
        {
            await _context.BlackLists.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> IsBlockedExistsAsync(
            Guid myId,
            Guid blackId,
            CancellationToken cancellationToken = default) =>
            _context.BlackLists.AnyAsync(
                x => x.MyId == myId && x.BlackId == blackId,
                cancellationToken);

        public Task<bool> IsBlockedBetweenAsync(
            Guid userA,
            Guid userB,
            CancellationToken cancellationToken = default) =>
            _context.BlackLists.AnyAsync(
                x => (x.MyId == userA && x.BlackId == userB) ||
                     (x.MyId == userB && x.BlackId == userA),
                cancellationToken);

        public Task<BlackList?> GetBlockAsync(
            Guid myId,
            Guid blackId,
            CancellationToken cancellationToken = default) =>
            _context.BlackLists.FirstOrDefaultAsync(
                x => x.MyId == myId && x.BlackId == blackId,
                cancellationToken);

        public async Task RemoveBlockAsync(BlackList entity, CancellationToken cancellationToken = default)
        {
            _context.BlackLists.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<FriendList?> GetFriendAsync(
            Guid myId,
            Guid friendId,
            CancellationToken cancellationToken = default) =>
            _context.FriendLists.FirstOrDefaultAsync(
                x => x.MyId == myId && x.FriendId == friendId,
                cancellationToken);

        public async Task RemoveFriendAsync(FriendList friend, CancellationToken cancellationToken = default)
        {
            _context.FriendLists.Remove(friend);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateFriendAsync(FriendList friend, CancellationToken cancellationToken = default)
        {
            _context.FriendLists.Update(friend);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<User>> SearchUsersAsync(
            string? search,
            int page,
            int pageSize,
            IReadOnlyCollection<Guid> excludeUserIds,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Users
                .Include(u => u.MediaUsers)
                .AsQueryable();

            if (excludeUserIds.Count > 0)
                query = query.Where(u => !excludeUserIds.Contains(u.Id));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.Login.Contains(term) ||
                    (u.Tag != null && u.Tag.Contains(term)));
            }

            return query
                .OrderBy(u => u.Login)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
