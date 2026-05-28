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
    public class EfPostMediaRepository : IPostMediaRepository
    {
        private readonly DbContextPost _context;

        public EfPostMediaRepository(DbContextPost context)
        {
            _context = context;
        }

        public async Task AddAsync(PostMedia media)
        {
            await _context.PostMedias.AddAsync(media);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostMedia>> GetByPostIdAsync(Guid postId)
        {
            return await _context.PostMedias
                .Where(x => x.PostId == postId)
                .ToListAsync();
        }
        public async Task DeleteAsync(PostMedia media)
        {
            _context.PostMedias.Remove(media);
            await _context.SaveChangesAsync();
        }
        public void DeleteRange(List<PostMedia> media)
        {
            _context.PostMedias.RemoveRange(media);
        }
    }
}
