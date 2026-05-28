using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostService.Application.DTO
{
    public class PostFullDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? UserLogin { get; set; }
        public string? UserTag { get; set; }
        public string? UserAvatar { get; set; }

        public List<PostMediaDto> Media { get; set; } = [];

        public int LikesCount { get; set; }
        public int FavoritesCount { get; set; }
        public int CommentsCount { get; set; }

        public bool IsLiked { get; set; }
        public bool IsFavorite { get; set; }
    }
}
