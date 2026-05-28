using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostService.Application.DTO
{
    public class PostProfileCard
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }

        // NEW
        public int LikesCount { get; set; }
    }
}
