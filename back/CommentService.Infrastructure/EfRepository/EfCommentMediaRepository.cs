using CommentService.Domain.IRepository;
using CommentService.Domain.Models;
using CommentService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace CommentService.Infrastructure.EfRepository
{
    public class EfCommentMediaRepository : ICommentMediaRepository
    {
        private readonly CommentDbContext _context;

        public EfCommentMediaRepository(CommentDbContext context)
        {
            _context = context;
        }

        public Task<CommentMedia?> GetByCommentIdAsync(
            Guid commentId,
            CancellationToken cancellationToken = default) =>
            _context.CommentMedia
                .FirstOrDefaultAsync(x => x.CommentId == commentId, cancellationToken);

        public async Task AddAsync(CommentMedia media, CancellationToken cancellationToken = default)
        {
            await _context.CommentMedia.AddAsync(media, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, CommentMedia>> GetByCommentIdsAsync(
            IReadOnlyCollection<Guid> commentIds,
            CancellationToken cancellationToken = default)
        {
            if (commentIds.Count == 0)
                return new Dictionary<Guid, CommentMedia>();

            var media = await _context.CommentMedia
                .Where(x => commentIds.Contains(x.CommentId))
                .ToListAsync(cancellationToken);

            return media.ToDictionary(x => x.CommentId);
        }

        public async Task DeleteAsync(CommentMedia media, CancellationToken cancellationToken = default)
        {
            _context.CommentMedia.Remove(media);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
