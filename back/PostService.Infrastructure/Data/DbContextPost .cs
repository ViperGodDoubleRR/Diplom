using Microsoft.EntityFrameworkCore;

using PostService.Domain.Models;
using PostService.Infrastructure.Configure;

namespace PostService.Infrastructure.Data
{
    public class DbContextPost : DbContext
    {
        public DbContextPost(DbContextOptions<DbContextPost> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<PostMedia> PostMedias { get; set; }

        public DbSet<PostReaction> PostReactions { get; set; }

        public DbSet<LikedPost> LikedPosts { get; set; }

        public DbSet<FavoritePost> FavoritePosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PostConfigure());
            modelBuilder.ApplyConfiguration(new PostMediaConfigure());
            modelBuilder.ApplyConfiguration(new PostReactionConfigure());
            modelBuilder.ApplyConfiguration(new LikedPostConfigure());
            modelBuilder.ApplyConfiguration(new FavoritePostConfigure());

            base.OnModelCreating(modelBuilder);
        }
    }
}
