using Microsoft.EntityFrameworkCore;

using UserService.Domain.IRepository;
using UserService.Domain.Models;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.EfRepository
{
    public class EfMediaRepository : IMediaRepository
    {
        private readonly DbContextUser _context;

        public EfMediaRepository(DbContextUser context)
        {
            _context = context;
        }

        public async Task AddAsync(MediaUser media, CancellationToken cancellationToken = default)
        {
            await _context.MediaUsers.AddAsync(media, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(MediaUser media, CancellationToken cancellationToken = default)
        {
            _context.MediaUsers.Remove(media);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteRangeAsync(
            IEnumerable<MediaUser> media,
            CancellationToken cancellationToken = default)
        {
            _context.MediaUsers.RemoveRange(media);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<MediaUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            _context.MediaUsers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public Task<List<MediaUser>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            _context.MediaUsers
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

        public Task<List<MediaUser>> GetByUserIdsAndTypeAsync(
            IReadOnlyCollection<Guid> userIds,
            string mediaType,
            CancellationToken cancellationToken = default)
        {
            if (userIds.Count == 0)
                return Task.FromResult(new List<MediaUser>());

            return _context.MediaUsers
                .Where(x => userIds.Contains(x.UserId) && x.MediaType == mediaType)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<List<MediaUser>> GetProfileMediaByUserIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            if (userIds.Count == 0)
                return Task.FromResult(new List<MediaUser>());

            return _context.MediaUsers
                .Where(x =>
                    userIds.Contains(x.UserId) &&
                    (x.MediaType == "avatar" || x.MediaType == "video"))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<List<MediaUser>> GetByUserIdAndTypeAsync(
            Guid userId,
            string mediaType,
            CancellationToken cancellationToken = default) =>
            _context.MediaUsers
                .Where(x => x.UserId == userId && x.MediaType == mediaType)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

        public async Task UpdateAsync(MediaUser media, CancellationToken cancellationToken = default)
        {
            _context.MediaUsers.Update(media);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
