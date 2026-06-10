using ChatService.Domain.IRepository;
using ChatService.Domain.Models;
using ChatService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.EfRepository
{
    public class EfChatMediaRepository : IChatMediaRepository
    {
        private readonly ChatDbContext _context;

        public EfChatMediaRepository(ChatDbContext context)
        {
            _context = context;
        }

        public Task<ChatMedia?> GetAvatarAsync(int chatId, CancellationToken cancellationToken = default) =>
            _context.ChatMedia
                .Where(m => m.ChatId == chatId && m.MediaType == "avatar")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        public Task<ChatMedia?> GetByIdAsync(int mediaId, CancellationToken cancellationToken = default) =>
            _context.ChatMedia.FirstOrDefaultAsync(m => m.Id == mediaId, cancellationToken);

        public Task<List<ChatMedia>> GetByChatIdAsync(int chatId, CancellationToken cancellationToken = default) =>
            _context.ChatMedia
                .AsNoTracking()
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(ChatMedia media, CancellationToken cancellationToken = default)
        {
            await _context.ChatMedia.AddAsync(media, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(ChatMedia media, CancellationToken cancellationToken = default)
        {
            _context.ChatMedia.Remove(media);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAllByChatIdAsync(int chatId, CancellationToken cancellationToken = default)
        {
            var items = await _context.ChatMedia
                .Where(m => m.ChatId == chatId)
                .ToListAsync(cancellationToken);

            if (items.Count == 0) return;

            _context.ChatMedia.RemoveRange(items);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
