using Microsoft.EntityFrameworkCore;

using PostService.Domain.IRepository;
using PostService.Domain.Models;
using PostService.Infrastructure.Data;

namespace PostService.Infrastructure.EfRepository
{
    public class EfPostMediaRepository : IPostMediaRepository
    {
        private readonly DbContextPost _context;

        public EfPostMediaRepository(DbContextPost context)
        {
            _context = context;
        }

        public async Task AddAsync(PostMedia media, CancellationToken cancellationToken = default)
        {
            await _context.PostMedias.AddAsync(media, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<PostMedia>> GetByPostIdAsync(
            Guid postId,
            CancellationToken cancellationToken = default) =>
            _context.PostMedias
                .Where(x => x.PostId == postId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

        public async Task<Dictionary<Guid, List<PostMedia>>> GetByPostIdsAsync(
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken = default)
        {
            if (postIds.Count == 0)
                return new Dictionary<Guid, List<PostMedia>>();

            var media = await _context.PostMedias
                .Where(x => postIds.Contains(x.PostId))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return media
                .GroupBy(x => x.PostId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task DeleteAsync(PostMedia media, CancellationToken cancellationToken = default)
        {
            _context.PostMedias.Remove(media);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteRangeAsync(
            IEnumerable<PostMedia> media,
            CancellationToken cancellationToken = default)
        {
            var list = media.ToList();
            if (list.Count == 0)
                return;

            _context.PostMedias.RemoveRange(list);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
