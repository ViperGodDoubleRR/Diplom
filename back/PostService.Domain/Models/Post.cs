using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PostService.Domain.Models
{
    public class Post
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        // navigation
        public List<PostMedia> Media { get; set; } = [];

        public List<PostReaction> Reactions { get; set; } = [];

        public List<LikedPost> Likes { get; set; } = [];

        public List<FavoritePost> Favorites { get; set; } = [];
    }
}
