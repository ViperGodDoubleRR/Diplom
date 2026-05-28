using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // =========================
        // FRIENDS
        // =========================

        public async Task<List<FriendList>> GetFriendsAsync(Guid userId)
        {
            return await _context.FriendLists
                .Include(x => x.Friend)
                .Where(x => x.MyId == userId)
                .ToListAsync();
        }

        public async Task AddFriendAsync(FriendList friend)
        {
            await _context.FriendLists.AddAsync(friend);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsFriendExistsAsync(
            Guid myId,
            Guid friendId)
        {
            return await _context.FriendLists.AnyAsync(x =>
                x.MyId == myId &&
                x.FriendId == friendId);
        }
        // =========================
        // BLOCKED
        // =========================

        public async Task<List<BlackList>> GetBlockedAsync(Guid userId)
        {
            return await _context.BlackLists
                .Include(x => x.Black)
                .Where(x => x.MyId == userId)
                .ToListAsync();
        }
        public async Task AddBlockAsync(BlackList entity)
        {
            await _context.BlackLists.AddAsync(entity);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBlockedExistsAsync(Guid myId,Guid blackId)
        {
            return await _context.BlackLists.AnyAsync(x =>
                x.MyId == myId &&
                x.BlackId == blackId);
        }

        public async Task<BlackList?> GetBlockAsync(Guid myId,Guid blackId)
        {
            return await _context.BlackLists
                .FirstOrDefaultAsync(x =>
                    x.MyId == myId &&
                    x.BlackId == blackId);
        }

        public async Task RemoveBlockAsync(
            BlackList entity)
        {
            _context.BlackLists.Remove(entity);

            await _context.SaveChangesAsync();
        }
        public async Task<FriendList?> GetFriendAsync(
    Guid myId,
    Guid friendId)
        {
            return await _context.FriendLists
                .FirstOrDefaultAsync(x =>
                    x.MyId == myId &&
                    x.FriendId == friendId);
        }

        public async Task RemoveFriendAsync(
            FriendList friend)
        {
            _context.FriendLists.Remove(friend);

            await _context.SaveChangesAsync();
        }
        public async Task<List<User>> SearchUsersAsync(string search,int page,  int pageSize,Guid myId)
        {
            var query = _context.Users
                .Include(u => u.MediaUsers)
                .AsQueryable();

            // исключаем себя
            query = query.Where(u => u.Id != myId);

            // поиск
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Login.Contains(search) ||
                    (u.Tag != null && u.Tag.Contains(search)));
            }

            // друзья
            var friendsIds = await _context.FriendLists
                .Where(f => f.MyId == myId)
                .Select(f => f.FriendId)
                .ToListAsync();

            query = query.Where(u => !friendsIds.Contains(u.Id));

            // блоки
            var blockedIds = await _context.BlackLists
                .Where(b => b.MyId == myId)
                .Select(b => b.BlackId)
                .ToListAsync();

            query = query.Where(u => !blockedIds.Contains(u.Id));

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
