using Microsoft.EntityFrameworkCore;

using PostService.Domain.IRepository;
using PostService.Domain.Models;
using PostService.Infrastructure.Data;

namespace PostService.Infrastructure.EfRepository
{
    public class EfPostRepository : IPostRepository
    {
        private readonly DbContextPost _context;

        public EfPostRepository(DbContextPost context)
        {
            _context = context;
        }

        public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
        {
            await _context.Posts.AddAsync(post, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Posts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public Task<bool> ExistsActiveAsync(Guid postId, CancellationToken cancellationToken = default) =>
            _context.Posts.AnyAsync(x => x.Id == postId && !x.IsDeleted, cancellationToken);

        public Task<List<Post>> GetUserPostsAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            _context.Posts
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        public async Task<Dictionary<Guid, int>> GetLikesCountsAsync(
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken = default)
        {
            if (postIds.Count == 0)
                return new Dictionary<Guid, int>();

            return await _context.LikedPosts
                .Where(x => postIds.Contains(x.PostId))
                .GroupBy(x => x.PostId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<Guid, int>> GetFavoritesCountsAsync(
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken = default)
        {
            if (postIds.Count == 0)
                return new Dictionary<Guid, int>();

            return await _context.FavoritePosts
                .Where(x => postIds.Contains(x.PostId))
                .GroupBy(x => x.PostId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<HashSet<Guid>> GetLikedPostIdsAsync(
            IReadOnlyCollection<Guid> postIds,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (postIds.Count == 0)
                return [];

            var ids = await _context.LikedPosts
                .Where(x => x.UserId == userId && postIds.Contains(x.PostId))
                .Select(x => x.PostId)
                .ToListAsync(cancellationToken);

            return ids.ToHashSet();
        }

        public async Task<HashSet<Guid>> GetFavoritePostIdsAsync(
            IReadOnlyCollection<Guid> postIds,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (postIds.Count == 0)
                return [];

            var ids = await _context.FavoritePosts
                .Where(x => x.UserId == userId && postIds.Contains(x.PostId))
                .Select(x => x.PostId)
                .ToListAsync(cancellationToken);

            return ids.ToHashSet();
        }

        public Task<bool> IsPostLikedAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            _context.LikedPosts.AnyAsync(
                x => x.PostId == postId && x.UserId == userId,
                cancellationToken);

        public Task<bool> IsPostFavoriteAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            _context.FavoritePosts.AnyAsync(
                x => x.PostId == postId && x.UserId == userId,
                cancellationToken);

        public async Task LikePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var exists = await IsPostLikedAsync(postId, userId, cancellationToken);
            if (exists)
                return;

            await _context.LikedPosts.AddAsync(new LikedPost
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UnlikePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var like = await _context.LikedPosts.FirstOrDefaultAsync(
                x => x.PostId == postId && x.UserId == userId,
                cancellationToken);

            if (like is null)
                return;

            _context.LikedPosts.Remove(like);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task FavoritePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var exists = await IsPostFavoriteAsync(postId, userId, cancellationToken);
            if (exists)
                return;

            await _context.FavoritePosts.AddAsync(new FavoritePost
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UnfavoritePostAsync(
            Guid postId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var favorite = await _context.FavoritePosts.FirstOrDefaultAsync(
                x => x.PostId == postId && x.UserId == userId,
                cancellationToken);

            if (favorite is null)
                return;

            _context.FavoritePosts.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<Post>> GetFavoritePostsAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            _context.FavoritePosts
                .Where(x => x.UserId == userId)
                .Include(x => x.Post)
                .Select(x => x.Post)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        public Task<List<Post>> GetLikedPostsAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            _context.LikedPosts
                .Where(x => x.UserId == userId)
                .Include(x => x.Post)
                .Select(x => x.Post)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        public async Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
        {
            post.UpdatedAt = DateTime.UtcNow;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
