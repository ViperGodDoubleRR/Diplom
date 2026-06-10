using ChatService.Domain.IRepository;
using ChatService.Domain.Models;
using ChatService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.EfRepository
{
    public class EfChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;

        public EfChatRepository(ChatDbContext context)
        {
            _context = context;
        }

        public Task<Chat?> GetByIdAsync(int chatId, CancellationToken cancellationToken = default) =>
            _context.Chats
                .Include(c => c.Users)
                .Include(c => c.Media)
                .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        public Task<Chat?> GetPrivateChatBetweenAsync(
            Guid userId1,
            Guid userId2,
            CancellationToken cancellationToken = default) =>
            _context.Chats
                .Include(c => c.Users)
                .Where(c => c.Type == ChatType.Private)
                .Where(c => c.Users.Any(u => u.UserId == userId1))
                .Where(c => c.Users.Any(u => u.UserId == userId2))
                .Where(c => c.Users.Count == 2)
                .FirstOrDefaultAsync(cancellationToken);

        public Task<List<Chat>> GetUserChatsAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            _context.Chats
                .AsNoTracking()
                .Where(c => c.Users.Any(u => u.UserId == userId))
                .Include(c => c.Users)
                .Include(c => c.Media)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

        public Task<List<Chat>> SearchPublicGroupsAsync(
            string search,
            int limit,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Chats
                .AsNoTracking()
                .Where(c => c.Type == ChatType.Group && c.IsPublic);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name != null && c.Name.Contains(search));
            }

            return query
                .Include(c => c.Media)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            await _context.Chats.AddAsync(chat, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> IsMemberAsync(
            int chatId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            _context.ChatUsers.AnyAsync(
                cu => cu.ChatId == chatId && cu.UserId == userId,
                cancellationToken);

        public async Task<ChatRole?> GetMemberRoleAsync(
            int chatId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var member = await _context.ChatUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    cu => cu.ChatId == chatId && cu.UserId == userId,
                    cancellationToken);

            return member?.Role;
        }

        public async Task AddMemberAsync(ChatUser member, CancellationToken cancellationToken = default)
        {
            await _context.ChatUsers.AddAsync(member, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveMemberAsync(
            int chatId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var member = await _context.ChatUsers
                .FirstOrDefaultAsync(
                    cu => cu.ChatId == chatId && cu.UserId == userId,
                    cancellationToken);

            if (member is null)
                return;

            _context.ChatUsers.Remove(member);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int chatId, CancellationToken cancellationToken = default)
        {
            var chat = await _context.Chats
                .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);

            if (chat is null)
                return;

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
