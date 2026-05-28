using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using UserService.Domain.Models;

namespace UserService.Domain.IRepository
{
    public interface IUserRepository
    {
        Task CreateUser(User user, CancellationToken cancellationToken);
        Task<User?> GetByIdAsync(Guid id);
        Task UpdateAsync(User user);
    }
}
