using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task AddAsync(MediaUser media)
        {
            await _context.MediaUsers.AddAsync(media);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MediaUser media)
        {
            _context.MediaUsers.Remove(media);
            await _context.SaveChangesAsync();
        }

        public async Task<MediaUser?> GetByIdAsync(int id)
        {
            return await _context.MediaUsers
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<List<MediaUser>> GetByUserIdAsync(Guid userId)
        {
            return await _context.MediaUsers
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        public async Task UpdateAsync(MediaUser media)
        {
            _context.MediaUsers.Update(media);
            await _context.SaveChangesAsync();
        }
    }
}
