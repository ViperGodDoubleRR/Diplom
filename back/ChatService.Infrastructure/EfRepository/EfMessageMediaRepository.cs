using ChatService.Domain.IRepository;
using ChatService.Domain.Models;
using ChatService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.EfRepository
{
    public class EfMessageMediaRepository : IMessageMediaRepository
    {
        private readonly ChatDbContext _context;

        public EfMessageMediaRepository(ChatDbContext context)
        {
            _context = context;
        }

        public Task<List<MessageMedia>> GetByMessageIdsAsync(
            IReadOnlyCollection<int> messageIds,
            CancellationToken cancellationToken = default)
        {
            if (messageIds.Count == 0)
                return Task.FromResult(new List<MessageMedia>());

            return _context.MessageMedia
                .AsNoTracking()
                .Where(m => messageIds.Contains(m.MessageId))
                .ToListAsync(cancellationToken);
        }

        public Task<List<MessageMedia>> GetByChatIdAsync(
            int chatId,
            CancellationToken cancellationToken = default) =>
            _context.MessageMedia
                .AsNoTracking()
                .Where(m => m.Message.ChatId == chatId)
                .ToListAsync(cancellationToken);

        public Task<MessageMedia?> GetByIdAsync(int mediaId, CancellationToken cancellationToken = default) =>
            _context.MessageMedia.FirstOrDefaultAsync(m => m.Id == mediaId, cancellationToken);

        public async Task AddAsync(MessageMedia media, CancellationToken cancellationToken = default)
        {
            await _context.MessageMedia.AddAsync(media, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(MessageMedia media, CancellationToken cancellationToken = default)
        {
            _context.MessageMedia.Remove(media);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
