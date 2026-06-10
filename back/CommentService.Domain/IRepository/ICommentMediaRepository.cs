using CommentService.Domain.Models;

namespace CommentService.Domain.IRepository
{
    public interface ICommentMediaRepository
    {
        Task<CommentMedia?> GetByCommentIdAsync(
            Guid commentId,
            CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, CommentMedia>> GetByCommentIdsAsync(
            IReadOnlyCollection<Guid> commentIds,
            CancellationToken cancellationToken = default);

        Task AddAsync(CommentMedia media, CancellationToken cancellationToken = default);

        Task DeleteAsync(CommentMedia media, CancellationToken cancellationToken = default);
    }
}
