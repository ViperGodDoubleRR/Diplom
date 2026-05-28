using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostService.Application.DTO
{
    namespace PostService.Application.DTO
    {
        public class PostReactionCard
        {
            public Guid Id { get; set; }

            public string Description { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; }

            public string? MediaUrl { get; set; }
            public string? MediaType { get; set; }

            public int LikesCount { get; set; }
            public int FavoritesCount { get; set; }

            public bool IsLiked { get; set; }
            public bool IsFavorite { get; set; }

            // user info
            public string UserLogin { get; set; } = string.Empty;
            public string UserTag { get; set; } = string.Empty;
            public string? UserAvatar { get; set; }
            public Guid UserId { get; set; }

        }
    }
}
