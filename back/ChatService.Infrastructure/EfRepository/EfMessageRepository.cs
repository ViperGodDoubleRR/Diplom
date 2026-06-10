using ChatService.Domain.IRepository;
using ChatService.Domain.Models;
using ChatService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.EfRepository
{
    public class EfMessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _context;

        public EfMessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        public Task<Message?> GetByIdAsync(int messageId, CancellationToken cancellationToken = default) =>
            _context.Messages
                .Include(m => m.Media)
                .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        public async Task<List<Message>> GetMessagesPageAsync(
            int chatId,
            int? beforeMessageId,
            int limit,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Message> query = _context.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId);

            if (beforeMessageId.HasValue)
            {
                var pivot = await _context.Messages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == beforeMessageId.Value, cancellationToken);

                if (pivot is null)
                    return [];

                query = query.Where(m => m.CreatedAt < pivot.CreatedAt);
            }

            var page = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(limit)
                .Include(m => m.Media)
                .ToListAsync(cancellationToken);

            page.Reverse();
            return page;
        }

        public async Task<Dictionary<int, Message>> GetLastMessagesByChatIdsAsync(
            IReadOnlyCollection<int> chatIds,
            CancellationToken cancellationToken = default)
        {
            if (chatIds.Count == 0)
                return new Dictionary<int, Message>();

            var messages = await _context.Messages
                .AsNoTracking()
                .Where(m => chatIds.Contains(m.ChatId))
                .GroupBy(m => m.ChatId)
                .Select(g => g.OrderByDescending(m => m.CreatedAt).First())
                .ToListAsync(cancellationToken);

            return messages.ToDictionary(m => m.ChatId);
        }

        public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            await _context.Messages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Message message, CancellationToken cancellationToken = default)
        {
            message.UpdatedAt = DateTime.UtcNow;
            _context.Messages.Update(message);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<Message>> GetActiveByChatIdAsync(
            int chatId,
            CancellationToken cancellationToken = default) =>
            _context.Messages
                .Include(m => m.Media)
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .ToListAsync(cancellationToken);

        public async Task SoftDeleteAllByChatIdAsync(
            int chatId,
            CancellationToken cancellationToken = default)
        {
            await _context.Messages
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(m => m.IsDeleted, true)
                        .SetProperty(m => m.Text, string.Empty)
                        .SetProperty(m => m.UpdatedAt, DateTime.UtcNow),
                    cancellationToken);
        }
    }
}
