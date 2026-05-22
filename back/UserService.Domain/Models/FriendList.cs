using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Models
{
    public class FriendList
    {
        public User My { get; set; } = null!;
        public Guid MyId { get; set; }
        public User Friend {  get; set; } = null!;
        public Guid FriendId {  get; set; }
        public string ChangeLogin { get; set; }=string.Empty;
    }
}
