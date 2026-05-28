using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task AddAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<List<Post>> GetUserPostsAsync(Guid userId, int page,int pageSize)
        {
            return await _context.Posts
                .Where(x => x.UserId == userId && x.IsDeleted==false)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<int> GetLikesCountAsync(Guid postId)
        {
            return await _context.LikedPosts
                .CountAsync(x => x.PostId == postId);
        }
        public async Task<List<Post>> GetUserPostsFeedAsync(Guid userId,int page,int pageSize)
        {
            return await _context.Posts
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<int> GetFavoritesCountAsync(Guid postId)
        {
            return await _context.FavoritePosts
                .CountAsync(x => x.PostId == postId);
        }
        public async Task<bool> IsPostLikedAsync(Guid postId, Guid userId)
        {
            return await _context.LikedPosts
                .AnyAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);
        }

        public async Task<bool> IsPostFavoriteAsync(Guid postId, Guid userId)
        {
            return await _context.FavoritePosts
                .AnyAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);
        }
        public async Task LikePostAsync(Guid postId, Guid userId)
        {
            var exists = await _context.LikedPosts
                .AnyAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);

            if (exists)
                return;

            await _context.LikedPosts.AddAsync(new LikedPost
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
        public async Task UnlikePostAsync(Guid postId, Guid userId)
        {
            var like = await _context.LikedPosts
                .FirstOrDefaultAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);

            if (like == null)
                return;

            _context.LikedPosts.Remove(like);

            await _context.SaveChangesAsync();
        }
        public async Task FavoritePostAsync(Guid postId, Guid userId)
        {
            var exists = await _context.FavoritePosts
                .AnyAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);

            if (exists)
                return;

            await _context.FavoritePosts.AddAsync(new FavoritePost
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId
            });

            await _context.SaveChangesAsync();
        }
        public async Task UnfavoritePostAsync(Guid postId, Guid userId)
        {
            var favorite = await _context.FavoritePosts
                .FirstOrDefaultAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);

            if (favorite == null)
                return;

            _context.FavoritePosts.Remove(favorite);

            await _context.SaveChangesAsync();
        }
        public async Task<List<Post>> GetFavoritePostsAsync(Guid userId, int page, int pageSize)
        {
            return await _context.FavoritePosts
                .Where(x => x.UserId == userId)
                .Include(x => x.Post)
                .Select(x => x.Post)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<List<Post>> GetLikedPostsAsync(Guid userId,int page,int pageSize)
        {
            return await _context.LikedPosts
                .Where(x => x.UserId == userId)
                .Include(x => x.Post)
                .Select(x => x.Post)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
