using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PostService.Domain.Models;

namespace PostService.Domain.IRepository
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<Post?> GetByIdAsync(Guid id);
        Task<List<Post>> GetUserPostsAsync(Guid userId, int page, int pageSize);
        Task<int> GetLikesCountAsync(Guid postId);
        Task<List<Post>> GetUserPostsFeedAsync(Guid userId, int page, int pageSize);
        Task<int> GetFavoritesCountAsync(Guid postId);
        Task<bool> IsPostLikedAsync(Guid postId, Guid userId);

        Task<bool> IsPostFavoriteAsync(Guid postId, Guid userId);
        Task LikePostAsync(Guid postId, Guid userId);
        Task UnlikePostAsync(Guid postId, Guid userId);

        Task FavoritePostAsync(Guid postId, Guid userId);
        Task UnfavoritePostAsync(Guid postId, Guid userId);
        Task<List<Post>> GetFavoritePostsAsync(Guid userId, int page, int pageSize);
        Task<List<Post>> GetLikedPostsAsync(Guid userId,int page,int pageSize);
        Task UpdateAsync(Post post);
    }
}
