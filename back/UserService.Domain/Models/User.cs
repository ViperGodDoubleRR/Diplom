using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string? Tag { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<FriendList> MyFriends { get; set; } = [];

        public List<FriendList> AddedToFriends { get; set; } = [];

        public List<BlackList> MyBlackList { get; set; } = [];

        public List<BlackList> BlockedByUsers { get; set; } = [];

        public List<MediaUser> MediaUsers { get; set; } = [];
    }
}
