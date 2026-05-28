using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UserService.Domain.Models;

namespace UserService.Domain.IRepository
{
    public interface ISocialRepository
    {
        Task<List<FriendList>> GetFriendsAsync(Guid userId);

        Task<List<BlackList>> GetBlockedAsync(Guid userId);
        Task AddFriendAsync(FriendList friend);

        Task<bool> IsFriendExistsAsync(Guid myId,Guid friendId);
        Task AddBlockAsync(BlackList entity);

        Task<bool> IsBlockedExistsAsync(Guid myId,Guid blackId);

        Task<BlackList?> GetBlockAsync(Guid myId,Guid blackId);

        Task RemoveBlockAsync(BlackList entity);
        Task<FriendList?> GetFriendAsync(Guid myId,Guid friendId);
        Task RemoveFriendAsync(
            FriendList friend);
        Task<List<User>> SearchUsersAsync(string search, int page,int pageSize,Guid myId);
    }
}
