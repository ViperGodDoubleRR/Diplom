using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UserService.Domain.Models;

namespace UserService.Domain.IRepository
{
    public interface IMediaRepository
    {
        Task AddAsync(MediaUser media);
        Task DeleteAsync(MediaUser media);
        Task<MediaUser?> GetByIdAsync(int id);
        Task<List<MediaUser>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(MediaUser media);

    }
}
