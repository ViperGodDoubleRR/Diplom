using PostService.Domain.Models;

namespace PostService.Domain.IRepository
{
    public interface IPostRepository
    {
        Task AddAsync(Post post, CancellationToken cancellationToken = default);
        Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsActiveAsync(Guid postId, CancellationToken cancellationToken = default);
        Task<List<Post>> GetUserPostsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> GetLikesCountsAsync(IReadOnlyCollection<Guid> postIds, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> GetFavoritesCountsAsync(IReadOnlyCollection<Guid> postIds, CancellationToken cancellationToken = default);
        Task<HashSet<Guid>> GetLikedPostIdsAsync(IReadOnlyCollection<Guid> postIds, Guid userId, CancellationToken cancellationToken = default);
        Task<HashSet<Guid>> GetFavoritePostIdsAsync(IReadOnlyCollection<Guid> postIds, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsPostLikedAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsPostFavoriteAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
        Task LikePostAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
        Task UnlikePostAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
        Task FavoritePostAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
        Task UnfavoritePostAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<Post>> GetFavoritePostsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<List<Post>> GetLikedPostsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
    }
}
