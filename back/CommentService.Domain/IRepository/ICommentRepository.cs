using CommentService.Domain.Models;

namespace CommentService.Domain.IRepository
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment, CancellationToken cancellationToken = default);

        Task<Comment?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<Comment>> GetRootCommentsByPostIdAsync(
            Guid postId,
            int offset,
            int limit,
            CancellationToken cancellationToken = default);

        Task<int> CountRootByPostIdAsync(
            Guid postId,
            CancellationToken cancellationToken = default);

        Task<int> CountAllByPostIdAsync(
            Guid postId,
            CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, int>> CountAllByPostIdsAsync(
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken = default);

        Task<List<Comment>> GetRepliesAsync(
            Guid parentId,
            int offset,
            int limit,
            CancellationToken cancellationToken = default);

        Task<int> GetRepliesCountAsync(
            Guid commentId,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default);

        Task<CommentReaction?> GetReactionAsync(
            Guid commentId,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task UpsertReactionAsync(
            CommentReaction reaction,
            CancellationToken cancellationToken = default);

        Task RemoveReactionAsync(
            CommentReaction reaction,
            CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, List<CommentReaction>>> GetReactionsByCommentIdsAsync(
            IReadOnlyCollection<Guid> commentIds,
            CancellationToken cancellationToken = default);
    }

}
