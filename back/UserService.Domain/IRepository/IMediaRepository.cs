using UserService.Domain.Models;

namespace UserService.Domain.IRepository
{
    public interface IMediaRepository
    {
        Task AddAsync(MediaUser media, CancellationToken cancellationToken = default);
        Task DeleteAsync(MediaUser media, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<MediaUser> media, CancellationToken cancellationToken = default);
        Task<MediaUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<MediaUser>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<MediaUser>> GetByUserIdsAndTypeAsync(
            IReadOnlyCollection<Guid> userIds,
            string mediaType,
            CancellationToken cancellationToken = default);
        Task<List<MediaUser>> GetProfileMediaByUserIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default);
        Task<List<MediaUser>> GetByUserIdAndTypeAsync(
            Guid userId,
            string mediaType,
            CancellationToken cancellationToken = default);
        Task UpdateAsync(MediaUser media, CancellationToken cancellationToken = default);
    }
}
