using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Models
{
    public class MediaUser
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;
        public Guid UserId { get; set; }
        public string Name { get; set; }=string.Empty;
        public string Type { get; set; }=string.Empty;
        public DateTime CreateAt {  get; set; }
    }
}
