using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PostService.Domain.Models;

namespace PostService.Domain.IRepository
{
    public interface IPostMediaRepository
    {
        Task AddAsync(PostMedia media);

        Task<List<PostMedia>> GetByPostIdAsync(Guid postId);
        Task DeleteAsync(PostMedia media);
        void DeleteRange(List<PostMedia> media); // 🔥 ДОБАВИТЬ

    }
}
