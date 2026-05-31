using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommentService.Domain.Models;
using CommentService.Infrastructure.Data;

using CommentService.Domain.IRepository;

using Microsoft.EntityFrameworkCore;

namespace CommentService.Infrastructure.EfRepository
{
    public class EfCommentRepository : ICommentRepository
    {
        private readonly CommentDbContext _context;

        public EfCommentRepository(CommentDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(x => x.PostId == postId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
