using PostService.Domain.Models;

namespace PostService.Domain.IRepository
{
    public interface IPostMediaRepository
    {
        Task AddAsync(PostMedia media, CancellationToken cancellationToken = default);
        Task<List<PostMedia>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, List<PostMedia>>> GetByPostIdsAsync(
            IReadOnlyCollection<Guid> postIds,
            CancellationToken cancellationToken = default);
        Task DeleteAsync(PostMedia media, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<PostMedia> media, CancellationToken cancellationToken = default);
    }
}
