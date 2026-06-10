using CommentService.Domain.IRepository;
using CommentService.Domain.Models;
using CommentService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace CommentService.Infrastructure.EfRepository
{
    public class EfCommentRepository : ICommentRepository
    {
        private readonly CommentDbContext _context;

        public EfCommentRepository(CommentDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            await _context.Comments.AddAsync(comment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<Comment?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            _context.Comments
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public Task<List<Comment>> GetRootCommentsByPostIdAsync(
            Guid postId,
            int offset,
            int limit,
            CancellationToken cancellationToken = default) =>
            _context.Comments
                .Where(x => x.PostId == postId && x.ParentId == null && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<int> CountRootByPostIdAsync(
            Guid postId,
            CancellationToken cancellationToken = default) =>
            _context.Comments.CountAsync(
                x => x.PostId == postId && x.ParentId == null && !x.IsDeleted,
                cancellationToken);

        public Task<int> CountAllByPostIdAsync(
            Guid postId,
            CancellationToken cancellationToken = default) =>
            _context.Comments.CountAsync(
                x => x.PostId == postId && !x.IsDeleted,
                cancellationToken);

        public async Task<Dictionary<Guid, int>> CountAllByPostIdsAsync(
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken = default)
        {
            if (postIds.Count == 0)
                return new Dictionary<Guid, int>();

            var counts = await _context.Comments
                .Where(x => postIds.Contains(x.PostId) && !x.IsDeleted)
                .GroupBy(x => x.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return counts.ToDictionary(x => x.PostId, x => x.Count);
        }

        public Task<List<Comment>> GetRepliesAsync(
            Guid parentId,
            int offset,
            int limit,
            CancellationToken cancellationToken = default) =>
            _context.Comments
                .Where(x => x.ParentId == parentId && !x.IsDeleted)
                .OrderBy(x => x.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<int> GetRepliesCountAsync(
            Guid commentId,
            CancellationToken cancellationToken = default) =>
            _context.Comments.CountAsync(
                x => x.ParentId == commentId && !x.IsDeleted,
                cancellationToken);

        public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<CommentReaction?> GetReactionAsync(
            Guid commentId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            _context.CommentReactions
                .FirstOrDefaultAsync(
                    x => x.CommentId == commentId && x.UserId == userId,
                    cancellationToken);

        public async Task UpsertReactionAsync(
            CommentReaction reaction,
            CancellationToken cancellationToken = default)
        {
            var existing = await GetReactionAsync(
                reaction.CommentId,
                reaction.UserId,
                cancellationToken);

            if (existing is null)
            {
                await _context.CommentReactions.AddAsync(reaction, cancellationToken);
            }
            else
            {
                existing.Type = reaction.Type;
                _context.CommentReactions.Update(existing);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveReactionAsync(
            CommentReaction reaction,
            CancellationToken cancellationToken = default)
        {
            _context.CommentReactions.Remove(reaction);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, List<CommentReaction>>> GetReactionsByCommentIdsAsync(
            IReadOnlyCollection<Guid> commentIds,
            CancellationToken cancellationToken = default)
        {
            if (commentIds.Count == 0)
                return new Dictionary<Guid, List<CommentReaction>>();

            var reactions = await _context.CommentReactions
                .Where(x => commentIds.Contains(x.CommentId))
                .ToListAsync(cancellationToken);

            return reactions
                .GroupBy(x => x.CommentId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
