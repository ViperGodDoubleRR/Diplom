using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Models
{
    public class BlackList
    {
        public User My { get; set; } = null!;
        public Guid MyId { get; set; }
        public User Black { get; set; } = null!;
        public Guid BlackId { get; set; }
    }
}
