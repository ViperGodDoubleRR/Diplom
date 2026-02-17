using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Models
{
    public class TableToken
    {
        public int Id { get; set; }

        public Guid TokenUserId { get; set; }   
        public User UserToken { get; set; } = null!;

        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

}
